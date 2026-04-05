using Kaesseli.Features.Integration;
using Kaesseli.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Kaesseli.Features.Automation;

internal class AutomationRepository : IAutomationRepository
{
    private readonly KaesseliContext _context;

    public AutomationRepository(KaesseliContext context) =>
        _context = context;

    public async Task<int> GetNrOfPossibleAutomation(string requestAutomationText, CancellationToken cancellationToken) =>
        (await GetOpenTransactionsMatching(requestAutomationText, cancellationToken)).Count;

    public async Task AddAutomation(AutomationEntry automationEntry, CancellationToken cancellationToken)
    {
        _context.Automations.Add(automationEntry);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<Transaction>> GetPossibleTransactions(string automationText, CancellationToken cancellationToken) =>
        await GetOpenTransactionsMatching(automationText, cancellationToken);

    public async Task<IEnumerable<AutomationEntry>> GetAutomations(CancellationToken cancellationToken)
    {
        var automations = await _context.Automations.ToListAsync(cancellationToken);
        var parts = await _context.Set<AutomationEntryPart>().ToListAsync(cancellationToken);
        var accounts = await _context.Accounts.ToListAsync(cancellationToken);
        var accountMap = accounts.ToDictionary(a => a.Id);

        foreach (var part in parts)
        {
            var accountFk = _context.Entry(part).Property<Guid>("AccountId").CurrentValue;
            if (accountMap.TryGetValue(accountFk, out var account))
                _context.Entry(part).Reference(p => p.Account).CurrentValue = account;
        }

        foreach (var automation in automations)
        {
            var relatedParts = parts
                .Where(p => _context.Entry(p).Property<Guid>("AutomationEntryId").CurrentValue == automation.Id)
                .ToList();
            _context.Entry(automation).Collection(a => a.Parts).CurrentValue = relatedParts;
        }

        return automations;
    }

    private async Task<List<Transaction>> GetOpenTransactionsMatching(string automationText, CancellationToken cancellationToken)
    {
        var pattern = "^" + System.Text.RegularExpressions.Regex.Escape(automationText)
                                   .Replace(@"\*", ".*")
                                   .Replace(@"\?", ".") + "$";
        var regex = new System.Text.RegularExpressions.Regex(
            pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        var allTransactions = await _context.Transactions.ToListAsync(cancellationToken);
        var journalEntries = await _context.JournalEntries.ToListAsync(cancellationToken);

        var transactionIdsWithJournal = journalEntries
            .Select(je => _context.Entry(je).Property<Guid?>("TransactionId").CurrentValue)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .ToHashSet();

        return allTransactions
            .Where(t => !transactionIdsWithJournal.Contains(t.Id))
            .Where(t => regex.IsMatch(t.Description))
            .ToList();
    }
}
