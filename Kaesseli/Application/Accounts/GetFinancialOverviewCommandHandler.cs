using System.Collections.Immutable;
using Kaesseli.Domain.Accounts;

namespace Kaesseli.Application.Accounts;

public interface IGetFinancialOverviewCommandHandler
{
    Task<GetFinancialOverviewCommandResult> Handle(GetFinancialOverviewCommand request, CancellationToken cancellationToken);
}

// ReSharper disable once UnusedType.Global
public class GetFinancialOverviewCommandHandler : IGetFinancialOverviewCommandHandler
{
    private readonly IGetAccountsSummaryQueryHandler _accountsSummaryHandler;

    public GetFinancialOverviewCommandHandler(IGetAccountsSummaryQueryHandler accountsSummaryHandler)
    {
        _accountsSummaryHandler = accountsSummaryHandler;
    }

    public async Task<GetFinancialOverviewCommandResult> Handle(
        GetFinancialOverviewCommand request,
        CancellationToken cancellationToken)
    {
        var accountSummary = (await _accountsSummaryHandler.Handle(
                                  request: new GetAccountsSummaryQuery { AccountingPeriodId = request.AccountingPeriodId },
                                  cancellationToken))
                             .GroupBy(account => account.TypeId)
                             .ToDictionary(account => account.Key, account => account.ToImmutableList());

        return new()
        {
            Expense = GetAccountTypeSummary(summaries: accountSummary[AccountType.Expense]),
            Revenue = GetAccountTypeSummary(summaries: accountSummary[AccountType.Revenue]),
            Liability = GetAccountTypeSummary(summaries: accountSummary[AccountType.Liability]),
            Asset = GetAccountTypeSummary(summaries: accountSummary[AccountType.Asset])
        };
    }

    private static AccountTypeSummary GetAccountTypeSummary(ImmutableList<GetAccountsSummaryQueryResult> summaries) =>
        new()
        {
            AccountBalance = summaries.Sum(summary => summary.AccountBalance),
            Budget = summaries.Sum(summary => summary.Budget),
            CurrentBudget = summaries.Sum(summary => summary.CurrentBudget),
            BudgetBalance = summaries.Sum(summary => summary.BudgetBalance),
            BudgetPerMonth = summaries.Sum(summary => summary.BudgetPerMonth),
            BudgetPerYear = summaries.Sum(summary => summary.BudgetPerYear)
        };
}
