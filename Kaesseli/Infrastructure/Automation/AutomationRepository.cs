using Kaesseli.Domain.Automation;
using Kaesseli.Domain.Integration;
using Kaesseli.Infrastructure.Common;
using Microsoft.EntityFrameworkCore;

namespace Kaesseli.Infrastructure.Automation;

internal class AutomationRepository : IAutomationRepository
{
    private readonly KaesseliContext _context;

    public AutomationRepository(KaesseliContext context) =>
        _context = context;

    public async Task<int> GetNrOfPossibleAutomation(string requestAutomationText, CancellationToken cancellationToken) =>
        await GetTransactionsQueryable(requestAutomationText)
            .CountAsync(cancellationToken);

    public async Task AddAutomation(AutomationEntry automationEntry, CancellationToken cancellationToken)
    {
        _context.Automations.Add(automationEntry);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<Transaction>> GetPossibleTransactions(string automationText, CancellationToken cancellationToken) =>
        await GetTransactionsQueryable(automationText)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<AutomationEntry>> GetAutomations(CancellationToken cancellationToken) =>
        await _context.Automations
            .Include(auto=> auto.Parts)
            .ThenInclude(part=> part.Account)
            .ToListAsync(cancellationToken);

    private IQueryable<Transaction> GetTransactionsQueryable(string automationText)
    {
        var pattern = "^" + System.Text.RegularExpressions.Regex.Escape(automationText)
                                   .Replace(@"\*", ".*")
                                   .Replace(@"\?", ".") + "$";
        var regex = new System.Text.RegularExpressions.Regex(
            pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        return _context.Transactions
                       .Where(tran => tran.JournalEntries!.Any() == false)
                       .AsEnumerable()
                       .Where(tran => regex.IsMatch(tran.Description))
                       .AsQueryable();
    }
}