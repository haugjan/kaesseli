using Kaesseli.Domain.Accounts;
using Kaesseli.Domain.Budget;
using Kaesseli.Domain.Journal;
using MediatR;

namespace Kaesseli.Application.Accounts;

// ReSharper disable once UnusedType.Global
public class GetAccountsSummaryQueryHandler(
    IAccountRepository accountRepo,
    IJournalRepository journalRepo,
    IBudgetRepository budgetRepo) : IRequestHandler<GetAccountsSummaryQuery, IEnumerable<GetAccountsSummaryQueryResult>>
{
    public async Task<IEnumerable<GetAccountsSummaryQueryResult>> Handle(
        GetAccountsSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var accounts = await accountRepo.GetAccounts(cancellationToken);

        var journalEntries = await journalRepo.GetJournalEntries(
                                 request: GetJournalEntriesRequest.Empty,
                                 cancellationToken);

        var budgetEntries = await budgetRepo.GetBudgetEntries(
                                request: GetBudgetEntriesRequest.Empty,
                                cancellationToken);

        return accounts.Select(account => GetAccountSummary(account, journalEntries, budgetEntries));
    }

    private static GetAccountsSummaryQueryResult GetAccountSummary(
        Account account,
        IEnumerable<JournalEntry> journalEntries,
        IEnumerable<BudgetEntry> budgetEntries)
    {
        var accountBalance = account.GetAccountBalance(journalEntries);
        var budget = account.GetBudget(budgetEntries);
        var budgetBalance = Account.GetBudgetBalance(budget, accountBalance);

        return account.ToAccountSummary(accountBalance, budget, budgetBalance);
    }

    
}