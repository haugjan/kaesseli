using Kaesseli.Features.AccountSuggestion.Gemini;
using Kaesseli.Features.Accounts;
using Kaesseli.Features.Integration;
using Kaesseli.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Kaesseli.Features.AccountSuggestion;

public static class GenerateAccountSuggestions
{
    public interface IRunner
    {
        bool TryStartInBackground();
        AccountSuggestionJobStatus Status { get; }
    }

    public class Runner(
        IServiceScopeFactory scopeFactory,
        AccountSuggestionJobStatus status,
        TimeProvider timeProvider,
        ILogger<Runner> logger
    ) : IRunner
    {
        public AccountSuggestionJobStatus Status => status;

        public bool TryStartInBackground()
        {
            var runId = Guid.NewGuid();
            // Reserve the slot synchronously with Total=0; the worker updates Total once it knows.
            if (!status.TryStart(runId, total: 0, timeProvider.GetUtcNow()))
                return false;

            _ = Task.Run(async () =>
            {
                try
                {
                    await RunAsync(runId, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Account suggestion job {RunId} crashed", runId);
                    status.IncrementFailed(ex.Message);
                }
                finally
                {
                    status.Finish(timeProvider.GetUtcNow());
                }
            });

            return true;
        }

        private async Task RunAsync(Guid runId, CancellationToken cancellationToken)
        {
            using var scope = scopeFactory.CreateScope();
            var sp = scope.ServiceProvider;
            var context = sp.GetRequiredService<KaesseliContext>();
            var suggestionRepo = sp.GetRequiredService<IAccountSuggestionRepository>();
            var accountRepo = sp.GetRequiredService<IAccountRepository>();
            var gemini = sp.GetRequiredService<IGeminiClient>();

            var openTransactions = await GetOpenTransactionsWithoutSuggestion(context, suggestionRepo, cancellationToken);
            status.SetTotal(openTransactions.Count);

            logger.LogInformation(
                "Account suggestion job {RunId} starting for {Count} transactions",
                runId,
                openTransactions.Count
            );

            if (openTransactions.Count == 0) return;

            var accounts = (await accountRepo.GetAccounts(cancellationToken)).ToList();
            var accountByShortName = accounts.ToDictionary(a => a.ShortName, StringComparer.OrdinalIgnoreCase);
            var accountPlan = accounts
                .Select(a => new GeminiAccountInfo(a.ShortName, a.Name, a.Type.ToString(), a.Number))
                .ToList();

            var examples = await BuildHistoricalExamples(context, accounts, cancellationToken);

            const int maxConsecutiveFailures = 5;
            var retryDelay = TimeSpan.FromSeconds(2);
            var consecutiveFailures = 0;

            foreach (var transaction in openTransactions)
            {
                if (cancellationToken.IsCancellationRequested) break;

                try
                {
                    var rawSuggestions = await gemini.SuggestAccountsAsync(
                        transaction.Description,
                        transaction.Amount,
                        accountPlan,
                        examples,
                        cancellationToken
                    );

                    var items = rawSuggestions
                        .Where(s => accountByShortName.ContainsKey(s.AccountShortName))
                        .Select((s, idx) => AccountSuggestionItem.Create(
                            accountByShortName[s.AccountShortName].Id,
                            Math.Clamp(s.Confidence, 0, 1),
                            rank: idx + 1
                        ))
                        .ToList();

                    var suggestion = AccountSuggestion.Create(
                        transaction.Id,
                        timeProvider.GetUtcNow(),
                        items,
                        error: items.Count == 0 ? "Keine gültigen Vorschläge erhalten." : null
                    );
                    await suggestionRepo.Add(suggestion, cancellationToken);
                    status.IncrementProcessed();
                    consecutiveFailures = 0;
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Suggestion failed for transaction {Id}", transaction.Id);
                    status.IncrementFailed(ex.Message);
                    consecutiveFailures++;

                    if (consecutiveFailures >= maxConsecutiveFailures)
                    {
                        logger.LogError(
                            "Aborting account suggestion job {RunId} after {Count} consecutive failures",
                            runId,
                            consecutiveFailures
                        );
                        break;
                    }

                    try
                    {
                        await Task.Delay(retryDelay, cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
            }

            logger.LogInformation(
                "Account suggestion job {RunId} finished: {Processed} processed, {Failed} failed",
                runId,
                status.Processed,
                status.Failed
            );
        }

        private static async Task<List<Transaction>> GetOpenTransactionsWithoutSuggestion(
            KaesseliContext context,
            IAccountSuggestionRepository suggestionRepo,
            CancellationToken cancellationToken
        )
        {
            var allTransactions = await context.Transactions.ToListAsync(cancellationToken);
            var journalEntries = await context.JournalEntries.ToListAsync(cancellationToken);

            var transactionIdsWithJournal = journalEntries
                .Select(je => context.Entry(je).Property<Guid?>("TransactionId").CurrentValue)
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .ToHashSet();

            var transactionIdsWithSuggestion = await suggestionRepo.GetTransactionIdsWithSuggestion(cancellationToken);

            return allTransactions
                .Where(t => !t.IsIgnored)
                .Where(t => !transactionIdsWithJournal.Contains(t.Id))
                .Where(t => !transactionIdsWithSuggestion.Contains(t.Id))
                .OrderBy(t => t.ValueDate)
                .ToList();
        }

        private static async Task<List<GeminiHistoricalAssignment>> BuildHistoricalExamples(
            KaesseliContext context,
            IReadOnlyList<Account> accounts,
            CancellationToken cancellationToken
        )
        {
            var accountById = accounts.ToDictionary(a => a.Id);
            var transactions = await context.Transactions.ToListAsync(cancellationToken);
            var transactionsById = transactions.ToDictionary(t => t.Id);

            var entries = await context.JournalEntries.ToListAsync(cancellationToken);

            var examples = new List<GeminiHistoricalAssignment>();
            foreach (var entry in entries)
            {
                var transactionId = context.Entry(entry).Property<Guid?>("TransactionId").CurrentValue;
                if (!transactionId.HasValue) continue;
                if (!transactionsById.TryGetValue(transactionId.Value, out var transaction)) continue;

                var creditId = context.Entry(entry).Property<Guid>("CreditAccountId").CurrentValue;
                var debitId = context.Entry(entry).Property<Guid>("DebitAccountId").CurrentValue;

                var summaryAccountId = transaction.TransactionSummary?.Account?.Id;
                Guid otherAccountId;
                if (summaryAccountId is { } sid)
                {
                    otherAccountId = sid == debitId ? creditId : debitId;
                }
                else
                {
                    otherAccountId = transaction.Amount >= 0 ? creditId : debitId;
                }

                if (!accountById.TryGetValue(otherAccountId, out var other)) continue;

                examples.Add(new GeminiHistoricalAssignment(
                    transaction.Description,
                    transaction.Amount,
                    other.ShortName
                ));
            }

            return examples;
        }
    }
}
