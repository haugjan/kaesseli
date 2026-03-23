using System.Collections.Immutable;
using Kaesseli.Domain.Accounts;

namespace Kaesseli.Application.Accounts;

public static class GetFinancialOverview
{
    public record Query
    {
        public required Guid AccountingPeriodId { get; init; }
    }

    public class Result
    {
        // ReSharper disable UnusedAutoPropertyAccessor.Global
        public required AccountTypeSummary Expense { get; init; }
        public required AccountTypeSummary Revenue { get; init; }
        public required AccountTypeSummary Liability { get; set; }
        public required AccountTypeSummary Asset { get; init; }
        // ReSharper restore UnusedAutoPropertyAccessor.Global
    }

    public class AccountTypeSummary
    {
        private readonly decimal? _budget;
        private decimal? _currentBudget;
        private readonly decimal? _budgetBalance;
        private readonly decimal? _budgetPerMonth;
        private readonly decimal? _budgetPerYear;

        // ReSharper disable UnusedAutoPropertyAccessor.Global
        // ReSharper disable UnusedMember.Global
        public required decimal AccountBalance { get; init; }

        public required decimal? Budget
        {
            get => _budget;
            init => _budget = value != 0 ? value : null;
        }

        public required decimal? BudgetPerMonth
        {
            get => _budgetPerYear;
            init => _budgetPerYear = value != 0 ? value : null;
        }

        public required decimal? BudgetPerYear
        {
            get => _budgetPerMonth;
            init => _budgetPerMonth = value != 0 ? value : null;
        }

        public required decimal? CurrentBudget
        {
            get => _currentBudget;
            set => _currentBudget = value != 0 ? value : null;
        }

        public required decimal? BudgetBalance
        {
            get => _budgetBalance;
            init => _budgetBalance = value != 0 ? value : null;
        }
        // ReSharper restore UnusedAutoPropertyAccessor.Global
        // ReSharper restore UnusedMember.Global
    }

    public interface IHandler
    {
        Task<Result> Handle(Query request, CancellationToken cancellationToken);
    }

    // ReSharper disable once UnusedType.Global
    public class Handler : IHandler
    {
        private readonly GetAccountsSummary.IHandler _accountsSummaryHandler;

        public Handler(GetAccountsSummary.IHandler accountsSummaryHandler)
        {
            _accountsSummaryHandler = accountsSummaryHandler;
        }

        public async Task<Result> Handle(Query request, CancellationToken cancellationToken)
        {
            var accountSummary = (await _accountsSummaryHandler.Handle(
                                      request: new GetAccountsSummary.Query { AccountingPeriodId = request.AccountingPeriodId },
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

        private static AccountTypeSummary GetAccountTypeSummary(ImmutableList<GetAccountsSummary.Result> summaries) =>
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
}
