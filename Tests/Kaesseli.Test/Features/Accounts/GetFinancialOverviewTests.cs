using Kaesseli.Contracts.Accounts;
using Kaesseli.Features.Accounts;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Kaesseli.Test.Features.Accounts;

public class GetFinancialOverviewTests
{
    [Fact]
    public async Task Handle_AggregatesAccountSummariesByType()
    {
        var summaries = new List<AccountOverview>
        {
            new(
                Guid.NewGuid(),
                "Lohn",
                "3000",
                "salary",
                "Work",
                "#8BC34A",
                "Einkommen",
                AccountType.Revenue,
                5500m,
                5500m,
                458.33m,
                5500m,
                1500m,
                4000m
            ),
            new(
                Guid.NewGuid(),
                "Lebensmittel",
                "4000",
                "groceries",
                "ShoppingCart",
                "#FF9800",
                "Ausgaben",
                AccountType.Expense,
                200m,
                600m,
                50m,
                600m,
                164m,
                436m
            ),
            new(
                Guid.NewGuid(),
                "Miete",
                "4010",
                "rent",
                "Home",
                "#9C27B0",
                "Ausgaben",
                AccountType.Expense,
                1500m,
                1500m,
                125m,
                1500m,
                411m,
                1089m
            ),
            new(
                Guid.NewGuid(),
                "Bank",
                "1000",
                "bank",
                "AccountBalance",
                "#1976D2",
                "Aktiv",
                AccountType.Asset,
                10000m,
                null,
                null,
                null,
                null,
                null
            ),
            new(
                Guid.NewGuid(),
                "Kreditkarte",
                "2000",
                "credit-card",
                "CreditCard",
                "#F44336",
                "Passiv",
                AccountType.Liability,
                65m,
                null,
                null,
                null,
                null,
                null
            ),
        };

        var mockHandler = Substitute.For<GetAccountsSummary.IHandler>();
        mockHandler
            .Handle(Arg.Any<GetAccountsSummary.Query>(), Arg.Any<CancellationToken>())
            .Returns(summaries);

        var handler = new GetFinancialOverview.Handler(mockHandler);
        var result = await handler.Handle(
            new GetFinancialOverview.Query(Guid.NewGuid()),
            CancellationToken.None
        );

        result.Revenue.AccountBalance.ShouldBe(5500m);
        result.Expense.AccountBalance.ShouldBe(1700m);
        result.Asset.AccountBalance.ShouldBe(10000m);
        result.Liability.AccountBalance.ShouldBe(65m);
        result.Expense.Budget.ShouldBe(2100m);
        result.Asset.Budget.ShouldBeNull();
    }

    [Fact]
    public async Task Handle_ZeroBudget_ReturnsNull()
    {
        var summaries = new List<AccountOverview>
        {
            new(
                Guid.NewGuid(),
                "Lohn",
                "3000",
                "salary",
                "Work",
                "#8BC34A",
                "Einkommen",
                AccountType.Revenue,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m
            ),
            new(
                Guid.NewGuid(),
                "Ausgaben",
                "4000",
                "expense",
                "ShoppingCart",
                "#FF9800",
                "Ausgaben",
                AccountType.Expense,
                0m,
                0m,
                0m,
                0m,
                0m,
                0m
            ),
            new(
                Guid.NewGuid(),
                "Bank",
                "1000",
                "bank",
                "AccountBalance",
                "#1976D2",
                "Aktiv",
                AccountType.Asset,
                0m,
                null,
                null,
                null,
                null,
                null
            ),
            new(
                Guid.NewGuid(),
                "KK",
                "2000",
                "credit-card",
                "CreditCard",
                "#F44336",
                "Passiv",
                AccountType.Liability,
                0m,
                null,
                null,
                null,
                null,
                null
            ),
        };

        var mockHandler = Substitute.For<GetAccountsSummary.IHandler>();
        mockHandler
            .Handle(Arg.Any<GetAccountsSummary.Query>(), Arg.Any<CancellationToken>())
            .Returns(summaries);

        var handler = new GetFinancialOverview.Handler(mockHandler);
        var result = await handler.Handle(
            new GetFinancialOverview.Query(Guid.NewGuid()),
            CancellationToken.None
        );

        result.Revenue.Budget.ShouldBeNull();
        result.Expense.Budget.ShouldBeNull();
        result.Revenue.CurrentBudget.ShouldBeNull();
    }
}
