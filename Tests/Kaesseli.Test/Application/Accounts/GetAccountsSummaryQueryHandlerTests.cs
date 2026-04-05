using Kaesseli.Features.Accounts;
using Kaesseli.Features.Budget;
using Kaesseli.Features.Journal;
using Microsoft.Extensions.Time.Testing;
using Moq;
using Shouldly;
using Xunit;

namespace Kaesseli.Test.Application.Accounts;

public class GetAccountsSummaryQueryHandlerTests
{
    private const decimal DebitAmount = 3;
    private const decimal CreditAmount = 5;
    private const decimal BudgetAmount = 7;

    [Theory]
    [InlineData(AccountType.Revenue, 2, -5)] //CreditAmount - DebitAmount
    [InlineData(AccountType.Expense, -2, 9)] //DebitAmount - CreditAmount
    public async Task Handle_WithBudget_ReturnsAccountSummary(
        AccountType accountType,
        decimal accountBalance,
        decimal budgetBalance
    )
    {
        // Arrange
        var accountRepo = new Mock<IAccountRepository>();
        var journalRepo = new Mock<IJournalRepository>();
        var budgetRepo = new Mock<IBudgetRepository>();
        var fakeTimeProvider = new FakeTimeProvider(new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero));
        var handler = new GetAccountsSummary.Handler(
            accountRepo.Object,
            journalRepo.Object,
            budgetRepo.Object,
            fakeTimeProvider
        );
        var cancellationToken = new CancellationToken();
        var periodId = Guid.NewGuid();

        var accountToTest = Account.Create("Current Account", accountType, new AccountIcon("favorite", "blue"));
        var otherAccount = Account.Create("Other Account", AccountType.Expense, new AccountIcon("favorite", "blue"));

        var journalEntries = CreateTestJournalEntries(accountToTest, otherAccount);
        var budgetEntries = CreateTestBudgetEntries(accountToTest);

        accountRepo
            .Setup(repo => repo.GetAccounts(cancellationToken))
            .ReturnsAsync(value: [otherAccount, accountToTest]);
        accountRepo
            .Setup(repo => repo.GetAccountingPeriod(periodId, cancellationToken))
            .ReturnsAsync(
                (Guid _, CancellationToken _) =>
                    AccountingPeriod.Create(
                        periodId.ToString(),
                        new DateOnly(year: 2023, month: 1, day: 1),
                        new DateOnly(year: 2024, month: 1, day: 1)
                    )
            );
        journalRepo
            .Setup(repo =>
                repo.GetJournalEntries(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid?>(),
                    It.IsAny<AccountType?>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(journalEntries);
        budgetRepo
            .Setup(repo =>
                repo.GetBudgetEntries(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid?>(),
                    It.IsAny<AccountType?>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(budgetEntries);

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
    public async Task Handle_WithoutBudget_ReturnsAccountSummary(
        AccountType accountType,
        decimal accountBalance
    )
    {
        // Arrange
        var accountRepo = new Mock<IAccountRepository>();
        var journalRepo = new Mock<IJournalRepository>();
        var budgetRepo = new Mock<IBudgetRepository>();
        var handler = new GetAccountsSummary.Handler(
            accountRepo.Object,
            journalRepo.Object,
            budgetRepo.Object,
            TimeProvider.System
        );
        var cancellationToken = new CancellationToken();
        var periodId = Guid.NewGuid();

        var otherAccount = Account.Create("Other Account", AccountType.Expense, new AccountIcon("favorite", "blue"));
        var accountToTest = Account.Create("Current Account", accountType, new AccountIcon("favorite", "blue"));

        var journalEntries = CreateTestJournalEntries(accountToTest, otherAccount);

        accountRepo
            .Setup(repo => repo.GetAccounts(It.IsAny<CancellationToken>()))
            .ReturnsAsync(value: [otherAccount, accountToTest]);
        accountRepo
            .Setup(repo => repo.GetAccountingPeriod(periodId, cancellationToken))
            .ReturnsAsync(
                (Guid _, CancellationToken _) =>
                    AccountingPeriod.Create(periodId.ToString(), default, default)
            );

        journalRepo
            .Setup(repo =>
                repo.GetJournalEntries(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid?>(),
                    It.IsAny<AccountType?>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(journalEntries);
        budgetRepo
            .Setup(repo =>
                repo.GetBudgetEntries(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid?>(),
                    It.IsAny<AccountType?>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(value: Array.Empty<BudgetEntry>());

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
