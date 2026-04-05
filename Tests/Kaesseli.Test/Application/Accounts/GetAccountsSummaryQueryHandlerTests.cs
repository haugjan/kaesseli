using Kaesseli.Features.Accounts;
using Kaesseli.Infrastructure;
using Kaesseli.Features.Budget;
using Kaesseli.Features.Journal;
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
        var dateTimeService = new Mock<IDateTimeService>();
        var handler = new GetAccountsSummary.Handler(
            accountRepo.Object,
            journalRepo.Object,
            budgetRepo.Object,
            dateTimeService.Object
        );
        var cancellationToken = new CancellationToken();
        var periodId = Guid.NewGuid();

        var accountToTest = new Account
        {
            Id = Guid.NewGuid(),
            Name = "Current Account",
            Type = accountType,
            Icon = new AccountIcon("favorite", "blue"),
        };
        var otherAccount = new Account
        {
            Id = Guid.NewGuid(),
            Name = "Other Account",
            Type = AccountType.Expense,
            Icon = new AccountIcon("favorite", "blue"),
        };

        var journalEntries = CreateTestJournalEntries(accountToTest, otherAccount);
        var budgetEntries = CreateTestBudgetEntries(accountToTest);

        accountRepo
            .Setup(repo => repo.GetAccounts(cancellationToken))
            .ReturnsAsync(value: [otherAccount, accountToTest]);
        accountRepo
            .Setup(repo => repo.GetAccountingPeriod(periodId, cancellationToken))
            .ReturnsAsync(
                (Guid _, CancellationToken _) =>
                    new AccountingPeriod
                    {
                        Id = periodId,
                        Description = periodId.ToString(),
                        FromInclusive = new DateOnly(year: 2023, month: 1, day: 1),
                        ToInclusive = new DateOnly(year: 2024, month: 1, day: 1),
                    }
            );
        dateTimeService.Setup(d => d.ToDay).Returns(new DateOnly(year: 2025, month: 1, day: 1));
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
        var dateTime = new Mock<IDateTimeService>();
        var handler = new GetAccountsSummary.Handler(
            accountRepo.Object,
            journalRepo.Object,
            budgetRepo.Object,
            dateTime.Object
        );
        var cancellationToken = new CancellationToken();
        var periodId = Guid.NewGuid();

        var otherAccount = new Account
        {
            Id = Guid.NewGuid(),
            Name = "Other Account",
            Type = AccountType.Expense,
            Icon = new AccountIcon("favorite", "blue"),
        };
        var accountToTest = new Account
        {
            Id = Guid.NewGuid(),
            Name = "Current Account",
            Type = accountType,
            Icon = new AccountIcon("favorite", "blue"),
        };

        var journalEntries = CreateTestJournalEntries(accountToTest, otherAccount);

        accountRepo
            .Setup(repo => repo.GetAccounts(It.IsAny<CancellationToken>()))
            .ReturnsAsync(value: [otherAccount, accountToTest]);
        accountRepo
            .Setup(repo => repo.GetAccountingPeriod(periodId, cancellationToken))
            .ReturnsAsync(
                (Guid _, CancellationToken _) =>
                    new AccountingPeriod
                    {
                        Id = periodId,
                        Description = periodId.ToString(),
                        FromInclusive = default,
                        ToInclusive = default,
                    }
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
            new()
            {
                Id = default,
                Description = "Budget 1",
                Amount = BudgetAmount,
                Account = accountToTest,
                AccountingPeriod = new AccountingPeriod
                {
                    Id = Guid.NewGuid(),
                    FromInclusive = default,
                    ToInclusive = default,
                    Description = string.Empty,
                },
            },
        ];

    private static IEnumerable<JournalEntry> CreateTestJournalEntries(
        Account accountToTest,
        Account otherAccount
    ) =>
        [
            new()
            {
                Id = Guid.NewGuid(),
                ValueDate = default,
                Description = "Test 1",
                Amount = DebitAmount,
                DebitAccount = accountToTest,
                CreditAccount = otherAccount,
                Transaction = null,
                AccountingPeriod = new AccountingPeriod
                {
                    Id = Guid.NewGuid(),
                    FromInclusive = default,
                    ToInclusive = default,
                    Description = string.Empty,
                },
            },
            new()
            {
                Id = Guid.NewGuid(),
                ValueDate = default,
                Description = "Test 2",
                Amount = CreditAmount,
                DebitAccount = otherAccount,
                CreditAccount = accountToTest,
                Transaction = null,
                AccountingPeriod = new AccountingPeriod
                {
                    Id = Guid.NewGuid(),
                    FromInclusive = default,
                    ToInclusive = default,
                    Description = string.Empty,
                },
            },
        ];
}
