using FluentAssertions;
using Kaesseli.Application.Accounts;
using Kaesseli.Domain.Accounts;
using Kaesseli.Domain.Budget;
using Kaesseli.Domain.Journal;
using Moq;
using Xunit;

namespace Kaesseli.Application.Test.Accounts;

public class GetAccountsSummaryQueryHandlerTests
{
    private const decimal DebitAmount = 3;
    private const decimal CreditAmount = 5;
    private const decimal BudgetAmount = 7;

    [Theory]
    [InlineData(AccountType.Revenue, 2, -5)] //CreditAmount - DebitAmount 
    [InlineData(AccountType.Expense, -2, 9)] //DebitAmount - CreditAmount   
    public async Task Handle_WithBudget_ReturnsAccountSummary(AccountType accountType, decimal accountBalance, decimal budgetBalance)
    {
        // Arrange
        var accountRepo = new Mock<IAccountRepository>();
        var journalRepo = new Mock<IJournalRepository>();
        var budgetRepo = new Mock<IBudgetRepository>();

        var handler = new GetAccountsSummaryQueryHandler(accountRepo.Object, journalRepo.Object, budgetRepo.Object);

        var accountToTest = new Account
        {
            Id = Guid.NewGuid(),
            Name = "Current Account",
            Type = accountType,
            Icon = "favorite",
            IconColor = "blue"
        };
        var otherAccount = new Account
        {
            Id = Guid.NewGuid(),
            Name = "Other Account",
            Type = AccountType.Expense,
            Icon = "favorite",
            IconColor = "blue"
        };

        var journalEntries = CreateTestJournalEntries(accountToTest, otherAccount);
        var budgetEntries = CreateTestBudgetEntries(accountToTest);

        accountRepo.Setup(repo => repo.GetAccounts(It.IsAny<CancellationToken>())).ReturnsAsync(value: [otherAccount, accountToTest]);
        journalRepo.Setup(repo => repo.GetJournalEntries(It.IsAny<GetJournalEntriesRequest>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync(journalEntries);
        budgetRepo.Setup(repo => repo.GetBudgetEntries(It.IsAny<GetBudgetEntriesRequest>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync(budgetEntries);

        var query = new GetAccountsSummaryQuery();

        // Act
        var result = (await handler.Handle(query, CancellationToken.None)).ToArray();

        // Assert
        result.Length.Should().Be(expected: 2);
        result.First().Id.Should().Be(otherAccount.Id);

        var summaryToTest = result[1];
        summaryToTest.Id.Should().Be(accountToTest.Id);
        summaryToTest.AccountBalance.Should().Be(accountBalance);
        summaryToTest.Budget.Should().Be(BudgetAmount);
        summaryToTest.BudgetBalance.Should().Be(expected: budgetBalance);
        summaryToTest.Name.Should().Be(accountToTest.Name);
        summaryToTest.Type.Should().Be(expected: accountToTest.Type.DisplayName());
        summaryToTest.TypeId.Should().Be(accountToTest.Type);
    }

    [Theory]
    [InlineData(AccountType.Asset, -2)] //DebitAmount - CreditAmount
    [InlineData(AccountType.Liability, 2)] //CreditAmount - DebitAmount 
    public async Task Handle_WithoutBudget_ReturnsAccountSummary(AccountType accountType, decimal accountBalance)
    {
        // Arrange
        var accountRepo = new Mock<IAccountRepository>();
        var journalRepo = new Mock<IJournalRepository>();
        var budgetRepo = new Mock<IBudgetRepository>();

        var handler = new GetAccountsSummaryQueryHandler(accountRepo.Object, journalRepo.Object, budgetRepo.Object);

        var otherAccount = new Account
        {
            Id = Guid.NewGuid(),
            Name = "Other Account",
            Type = AccountType.Expense,
            Icon = "favorite",
            IconColor = "blue"
        };
        var accountToTest = new Account
        {
            Id = Guid.NewGuid(),
            Name = "Current Account",
            Type = accountType,
            Icon = "favorite",
            IconColor = "blue"
        };

        var journalEntries = CreateTestJournalEntries(accountToTest, otherAccount);

        accountRepo.Setup(repo => repo.GetAccounts(It.IsAny<CancellationToken>())).ReturnsAsync(value: [otherAccount, accountToTest]);
        journalRepo.Setup(repo => repo.GetJournalEntries(It.IsAny<GetJournalEntriesRequest>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync(journalEntries);
        budgetRepo.Setup(repo => repo.GetBudgetEntries(It.IsAny<GetBudgetEntriesRequest>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync(value: Array.Empty<BudgetEntry>());

        var query = new GetAccountsSummaryQuery();

        // Act
        var result = (await handler.Handle(query, CancellationToken.None)).ToArray();

        // Assert
        result.Length.Should().Be(expected: 2);
        result.First().Id.Should().Be(otherAccount.Id);

        var summaryToTest = result[1];
        summaryToTest.Id.Should().Be(accountToTest.Id);
        summaryToTest.AccountBalance.Should().Be(accountBalance);
        summaryToTest.Budget.Should().BeNull();
        summaryToTest.BudgetBalance.Should().BeNull();
        summaryToTest.Name.Should().Be(accountToTest.Name);
        summaryToTest.Type.Should().Be(expected: accountToTest.Type.DisplayName());
        summaryToTest.TypeId.Should().Be(accountToTest.Type);
    }

    private static IEnumerable<BudgetEntry> CreateTestBudgetEntries(Account accountToTest) =>
    [
        new()
        {
            Id = default,
            ValueDate = default,
            Description = "Budget 1",
            Amount = BudgetAmount,
            Account = accountToTest
        }
    ];

    private static IEnumerable<JournalEntry> CreateTestJournalEntries(Account accountToTest, Account otherAccount) =>
    [
        new()
        {
            Id = Guid.NewGuid(),
            ValueDate = default,
            Description = "Test 1",
            Amount = DebitAmount,
            DebitAccount = accountToTest,
            CreditAccount = otherAccount,
            Transaction = null
        },

        new()
        {
            Id = Guid.NewGuid(),
            ValueDate = default,
            Description = "Test 2",
            Amount = CreditAmount,
            DebitAccount = otherAccount,
            CreditAccount = accountToTest,
            Transaction = null
        }
    ];
}