using Kaesseli.Features.Integration;
using Kaesseli.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Kaesseli.Features.Automation;

public interface IAutomationRepository
{
    Task<int> GetNrOfPossibleAutomation(
        string requestAutomationText,
        CancellationToken cancellationToken
    );
    Task AddAutomation(AutomationEntry entry, CancellationToken cancellationToken);
    Task<IEnumerable<Transaction>> GetPossibleTransactions(
        string automationText,
        CancellationToken cancellationToken
    );
    Task<IEnumerable<AutomationEntry>> GetAutomations(CancellationToken cancellationToken);
}

internal class AutomationRepository(KaesseliContext context) : IAutomationRepository
{
    public async Task<int> GetNrOfPossibleAutomation(
        string requestAutomationText,
        CancellationToken cancellationToken
    ) => (await GetOpenTransactionsMatching(requestAutomationText, cancellationToken)).Count;

    public async Task AddAutomation(
        AutomationEntry automationEntry,
        CancellationToken cancellationToken
    )
    {
        context.Automations.Add(automationEntry);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<Transaction>> GetPossibleTransactions(
        string automationText,
        CancellationToken cancellationToken
    ) => await GetOpenTransactionsMatching(automationText, cancellationToken);

    public async Task<IEnumerable<AutomationEntry>> GetAutomations(
        CancellationToken cancellationToken
    )
    {
        var automations = await context.Automations.ToListAsync(cancellationToken);
        var parts = await context.Set<AutomationEntryPart>().ToListAsync(cancellationToken);

        foreach (var automation in automations)
        {
            var relatedParts = parts
                .Where(p =>
                    context.Entry(p).Property<Guid>("AutomationEntryId").CurrentValue
                    == automation.Id
                )
                .ToList();
            context.Entry(automation).Collection(a => a.Parts).CurrentValue = relatedParts;
        }

        return automations;
    }

    private async Task<List<Transaction>> GetOpenTransactionsMatching(
        string automationText,
        CancellationToken cancellationToken
    )
    {
        var pattern =
            "^"
            + System
                .Text.RegularExpressions.Regex.Escape(automationText)
                .Replace(@"\*", ".*")
                .Replace(@"\?", ".")
            + "$";
        var regex = new System.Text.RegularExpressions.Regex(
            pattern,
            System.Text.RegularExpressions.RegexOptions.IgnoreCase
        );

        var allTransactions = await context.Transactions.ToListAsync(cancellationToken);
        var journalEntries = await context.JournalEntries.ToListAsync(cancellationToken);

        var transactionIdsWithJournal = journalEntries
            .Select(je => context.Entry(je).Property<Guid?>("TransactionId").CurrentValue)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .ToHashSet();

        return allTransactions
            .Where(t => !transactionIdsWithJournal.Contains(t.Id))
            .Where(t => t.Description is not null && regex.IsMatch(t.Description))
            .ToList();
    }
}
