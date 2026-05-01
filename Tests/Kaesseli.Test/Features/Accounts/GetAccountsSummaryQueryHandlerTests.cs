using Kaesseli.Features.Accounts;
using Kaesseli.Features.Budget;
using Kaesseli.Features.Journal;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Kaesseli.Test.Features.Accounts;

public class GetAccountsSummaryQueryHandlerTests
{
    private const decimal DebitAmount = 3;
    private const decimal CreditAmount = 5;
    private const decimal BudgetAmount = 7;

    [Theory]
    [InlineData(AccountType.Revenue, 2, -5)] //CreditAmount - DebitAmount
    [InlineData(AccountType.Expense, -2, 9)] //DebitAmount - CreditAmount
    public async Task Handle_WithBudget_ReturnsAccountOverview(
        AccountType accountType,
        decimal accountBalance,
        decimal budgetBalance
    )
    {
        // Arrange
        var accountRepo = Substitute.For<IAccountRepository>();
        var journalRepo = Substitute.For<IJournalRepository>();
        var budgetRepo = Substitute.For<IBudgetRepository>();
        var fakeTimeProvider = new FakeTimeProvider(
            new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)
        );
        var handler = new GetAccountsSummary.Handler(
            accountRepo,
            journalRepo,
            budgetRepo,
            fakeTimeProvider
        );
        var cancellationToken = new CancellationToken();
        var periodId = Guid.NewGuid();

        var accountToTest = AccountFactory.Create(
            "Current Account",
            accountType,
            new AccountIcon("favorite", "blue")
        );
        var otherAccount = AccountFactory.Create(
            "Other Account",
            AccountType.Expense,
            new AccountIcon("favorite", "blue")
        );

        var journalEntries = CreateTestJournalEntries(accountToTest, otherAccount);
        var budgetEntries = CreateTestBudgetEntries(accountToTest);

        accountRepo.GetAccounts(cancellationToken).Returns([otherAccount, accountToTest]);
        accountRepo
            .GetAccountingPeriod(periodId, cancellationToken)
            .Returns(
                AccountingPeriod.Create(
                    periodId.ToString(),
                    new DateOnly(year: 2023, month: 1, day: 1),
                    new DateOnly(year: 2024, month: 1, day: 1)
                )
            );
        journalRepo
            .GetJournalEntries(
                Arg.Any<Guid>(),
                Arg.Any<Guid?>(),
                Arg.Any<AccountType?>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(journalEntries);
        budgetRepo
            .GetBudgetEntries(
                Arg.Any<Guid>(),
                Arg.Any<Guid?>(),
                Arg.Any<AccountType?>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(budgetEntries);

        var query = new GetAccountsSummary.Query(periodId);

        // Act
        var result = (await handler.Handle(query, CancellationToken.None)).ToArray();

        // Assert
        result.Length.ShouldBe(2);
        result.First().Id.ShouldBe(otherAccount.Id);

        var summaryToTest = result[1];
        summaryToTest.Id.ShouldBe(accountToTest.Id);
        summaryToTest.AccountBalance.ShouldBe(accountBalance);
        Math.Round(summaryToTest.Budget!.Value, 2).ShouldBe(BudgetAmount);
        summaryToTest.BudgetBalance.ShouldBe(budgetBalance);
        summaryToTest.Name.ShouldBe(accountToTest.Name);
        summaryToTest.Type.ShouldBe(accountToTest.Type.DisplayName());
        summaryToTest.TypeId.ShouldBe(accountToTest.Type);
    }

    [Theory]
    [InlineData(AccountType.Asset, -2)] //DebitAmount - CreditAmount
    [InlineData(AccountType.Liability, 2)] //CreditAmount - DebitAmount
    public async Task Handle_WithoutBudget_ReturnsAccountOverview(
        AccountType accountType,
        decimal accountBalance
    )
    {
        // Arrange
        var accountRepo = Substitute.For<IAccountRepository>();
        var journalRepo = Substitute.For<IJournalRepository>();
        var budgetRepo = Substitute.For<IBudgetRepository>();
        var handler = new GetAccountsSummary.Handler(
            accountRepo,
            journalRepo,
            budgetRepo,
            TimeProvider.System
        );
        var cancellationToken = new CancellationToken();
        var periodId = Guid.NewGuid();

        var otherAccount = AccountFactory.Create(
            "Other Account",
            AccountType.Expense,
            new AccountIcon("favorite", "blue")
        );
        var accountToTest = AccountFactory.Create(
            "Current Account",
            accountType,
            new AccountIcon("favorite", "blue")
        );

        var journalEntries = CreateTestJournalEntries(accountToTest, otherAccount);

        accountRepo
            .GetAccounts(Arg.Any<CancellationToken>())
            .Returns([otherAccount, accountToTest]);
        accountRepo
            .GetAccountingPeriod(periodId, cancellationToken)
            .Returns(AccountingPeriod.Create(periodId.ToString(), default, default));

        journalRepo
            .GetJournalEntries(
                Arg.Any<Guid>(),
                Arg.Any<Guid?>(),
                Arg.Any<AccountType?>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(journalEntries);
        budgetRepo
            .GetBudgetEntries(
                Arg.Any<Guid>(),
                Arg.Any<Guid?>(),
                Arg.Any<AccountType?>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(Array.Empty<BudgetEntry>());

        var query = new GetAccountsSummary.Query(periodId);

        // Act
        var result = (await handler.Handle(query, cancellationToken)).ToArray();

        // Assert
        result.Length.ShouldBe(2);
        result.First().Id.ShouldBe(otherAccount.Id);

        var summaryToTest = result[1];
        summaryToTest.Id.ShouldBe(accountToTest.Id);
        summaryToTest.AccountBalance.ShouldBe(accountBalance);
        summaryToTest.Budget.ShouldBeNull();
        summaryToTest.BudgetBalance.ShouldBeNull();
        summaryToTest.Name.ShouldBe(accountToTest.Name);
        summaryToTest.Type.ShouldBe(accountToTest.Type.DisplayName());
        summaryToTest.TypeId.ShouldBe(accountToTest.Type);
    }

    private static IEnumerable<BudgetEntry> CreateTestBudgetEntries(Account accountToTest) =>
        [
            BudgetEntry.Create(
                description: "Budget 1",
                amount: BudgetAmount,
                account: accountToTest,
                accountingPeriod: AccountingPeriod.Create("Test Period", default, default)
            ),
        ];

    private static IEnumerable<JournalEntry> CreateTestJournalEntries(
        Account accountToTest,
        Account otherAccount
    ) =>
        [
            JournalEntry.Create(
                valueDate: default,
                description: "Test 1",
                amount: DebitAmount,
                debitAccount: accountToTest,
                creditAccount: otherAccount,
                accountingPeriod: AccountingPeriod.Create("Test Period", default, default)
            ),
            JournalEntry.Create(
                valueDate: default,
                description: "Test 2",
                amount: CreditAmount,
                debitAccount: otherAccount,
                creditAccount: accountToTest,
                accountingPeriod: AccountingPeriod.Create("Test Period", default, default)
            ),
        ];
}
