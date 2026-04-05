using System.Collections.Immutable;
using AccountsSummaryResult = Kaesseli.Contracts.Features.Accounts.GetAccountsSummaryContract.Result;
using Result = Kaesseli.Contracts.Features.Accounts.GetFinancialOverviewContract.Result;
using AccountTypeSummary = Kaesseli.Contracts.Features.Accounts.GetFinancialOverviewContract.AccountTypeSummary;

namespace Kaesseli.Features.Accounts;

public static class GetFinancialOverview
{
    public record Query(Guid AccountingPeriodId);

    public interface IHandler
    {
        Task<Result> Handle(Query request, CancellationToken cancellationToken);
    }

    // ReSharper disable once UnusedType.Global
    public class Handler(GetAccountsSummary.IHandler accountsSummaryHandler) : IHandler
    {
        public async Task<Result> Handle(Query request, CancellationToken cancellationToken)
        {
            var accountSummary = (await accountsSummaryHandler.Handle(
                                      request: new GetAccountsSummary.Query(request.AccountingPeriodId),
                                      cancellationToken))
                                 .GroupBy(account => account.TypeId)
                                 .ToDictionary(account => account.Key, account => account.ToImmutableList());

            return new Result(
                Expense: GetAccountTypeSummary(summaries: accountSummary[AccountType.Expense]),
                Revenue: GetAccountTypeSummary(summaries: accountSummary[AccountType.Revenue]),
                Liability: GetAccountTypeSummary(summaries: accountSummary[AccountType.Liability]),
                Asset: GetAccountTypeSummary(summaries: accountSummary[AccountType.Asset]));
        }

        private static AccountTypeSummary GetAccountTypeSummary(ImmutableList<AccountsSummaryResult> summaries)
        {
            static decimal? NullIfZero(decimal? v) => v is 0 ? null : v;

            return new AccountTypeSummary(
                AccountBalance: summaries.Sum(s => s.AccountBalance),
                Budget: NullIfZero(summaries.Sum(s => s.Budget)),
                BudgetPerMonth: NullIfZero(summaries.Sum(s => s.BudgetPerMonth)),
                BudgetPerYear: NullIfZero(summaries.Sum(s => s.BudgetPerYear)),
                CurrentBudget: NullIfZero(summaries.Sum(s => s.CurrentBudget)),
                BudgetBalance: NullIfZero(summaries.Sum(s => s.BudgetBalance)));
        }
    }
}
