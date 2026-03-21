using Kaesseli.Application.Utility;
using Kaesseli.Domain.Accounts;
using Kaesseli.Domain.Budget;
using Kaesseli.Domain.Journal;
using MediatR;

namespace Kaesseli.Application.Accounts;

// ReSharper disable once UnusedType.Global
public class GetAccountsSummaryQueryHandler(
    IAccountRepository accountRepo,
    IJournalRepository journalRepo,
    IBudgetRepository budgetRepo,
    IDateTimeService dateTimeService) : IRequestHandler<GetAccountsSummaryQuery, IEnumerable<GetAccountsSummaryQueryResult>>
{
    private readonly IDateTimeService _dateTimeService = dateTimeService;

    public async Task<IEnumerable<GetAccountsSummaryQueryResult>> Handle(
        GetAccountsSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var accounts = await accountRepo.GetAccounts(cancellationToken);
        var accountingPeriod = await accountRepo.GetAccountingPeriod(request.AccountingPeriodId, cancellationToken);

        var journalEntries = await journalRepo.GetJournalEntries(
                                 request.AccountingPeriodId, accountId: null, accountType: null,
                                 cancellationToken);

        var budgetEntries = await budgetRepo.GetBudgetEntries(
                                request.AccountingPeriodId, accountId: null, accountType: null,
                                cancellationToken);

        return accounts.Select(account => GetAccountSummary(account, journalEntries, budgetEntries, accountingPeriod));
    }

    private GetAccountsSummaryQueryResult GetAccountSummary(
        Account account,
        IEnumerable<JournalEntry> journalEntries,
        IEnumerable<BudgetEntry> budgetEntries,
        AccountingPeriod accountingPeriod)
    {
        var today = _dateTimeService.ToDay;
        budgetEntries = budgetEntries.ToArray();
        var accountBalance = AccountBalanceCalculator.GetAccountBalance(account, journalEntries);
        var budgetPerYear = AccountBalanceCalculator.GetBudgetPerYear(account, budgetEntries);
        var budget = AccountBalanceCalculator.GetBudget(account, budgetEntries, accountingPeriod);
        var budgetPerMonth = AccountBalanceCalculator.GetBudgetPerMonth(account, budgetEntries);
        var currentBudget = AccountBalanceCalculator.GetCurrentBudget(account, budgetEntries, accountingPeriod, today);
        var budgetBalance = AccountBalanceCalculator.GetBudgetBalance(account.Type, currentBudget, accountBalance);

        return account.ToAccountSummary(accountBalance, budget,budgetPerMonth, budgetPerYear, currentBudget, budgetBalance);
    }


}