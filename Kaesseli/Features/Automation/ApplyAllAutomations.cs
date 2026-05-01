using Kaesseli.Features.Accounts;
using Kaesseli.Features.Integration;
using Kaesseli.Features.Integration.NextOpenTransaction;

namespace Kaesseli.Features.Automation;

public static class ApplyAllAutomations
{
    public record Query(Guid AccountingPeriodId);

    public interface IHandler
    {
        Task Handle(Query request, CancellationToken cancellationToken);
    }

    public class Handler(
        IAutomationRepository automateRepository,
        IAccountRepository accountRepository,
        SplitOpenTransaction.IHandler splitHandler
    ) : IHandler
    {
        public async Task Handle(Query request, CancellationToken cancellationToken)
        {
            var automations = (await automateRepository.GetAutomations(cancellationToken)).ToList();
            if (automations.Count == 0)
                return;

            var accounts = await accountRepository.GetAccounts(cancellationToken);
            var accountByShortName = accounts.ToDictionary(a => a.ShortName);

            foreach (var automationEntry in automations)
                await ApplyAutomation(
                    request.AccountingPeriodId,
                    automationEntry,
                    accountByShortName,
                    cancellationToken
                );
        }

        private async Task ApplyAutomation(
            Guid accountingPeriodId,
            AutomationEntry automationEntry,
            IReadOnlyDictionary<string, Account> accountByShortName,
            CancellationToken cancellationToken
        )
        {
            var transactions = await automateRepository.GetPossibleTransactions(
                automationEntry.AutomationText,
                cancellationToken
            );
            foreach (var transaction in transactions)
                await ApplyAutomation(
                    accountingPeriodId,
                    automationEntry,
                    transaction,
                    accountByShortName,
                    cancellationToken
                );
        }

        private async Task ApplyAutomation(
            Guid accountingPeriodId,
            AutomationEntry automationEntry,
            Transaction transaction,
            IReadOnlyDictionary<string, Account> accountByShortName,
            CancellationToken cancellationToken
        )
        {
            var parts = automationEntry.Parts.ToList();
            var entries = parts
                .Take(parts.Count - 1)
                .Select(part => new SplitOpenTransactionEntry(
                    OtherAccountId: ResolveAccountId(part.AccountShortName, accountByShortName),
                    Amount: Math.Round(transaction.Amount * part.AmountProportion, decimals: 2)
                ))
                .ToList();

            var lastPart = parts.Last();
            var remainingAmount = transaction.Amount - entries.Sum(entry => entry.Amount);
            entries.Add(
                new SplitOpenTransactionEntry(
                    OtherAccountId: ResolveAccountId(lastPart.AccountShortName, accountByShortName),
                    Amount: remainingAmount
                )
            );

            await splitHandler.Handle(
                request: new SplitOpenTransaction.Query(
                    AccountingPeriodId: accountingPeriodId,
                    TransactionId: transaction.Id,
                    Entries: entries
                ),
                cancellationToken
            );
        }

        private static Guid ResolveAccountId(
            string shortName,
            IReadOnlyDictionary<string, Account> accountByShortName
        ) =>
            accountByShortName.TryGetValue(shortName, out var account)
                ? account.Id
                : throw new AutomationAccountShortNameNotFoundException(shortName);
    }
}
