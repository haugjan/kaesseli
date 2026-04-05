using Kaesseli.Features.Budget;
using Kaesseli.Features.Accounts;
using Kaesseli.Test.Faker;
using Moq;
using Xunit;

namespace Kaesseli.Test.Application.Budget;

public class AddJournalEntryCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldAddAccountSuccessfully()
    {
        // Arrange
        var mockRepo = new Mock<IBudgetRepository>();
        var accountRepo = new Mock<IAccountRepository>();
        var command = new SmartFaker<SetBudget.Query>().Generate();
        var cancellationToken = new CancellationToken();
        var account = Account.Create("Account", AccountType.Expense, new AccountIcon("favorite", "blue"));
        var accountingPeriod = AccountingPeriod.Create("Test Period", default, default);

        mockRepo
            .Setup(repo =>
                repo.SetBudget(
                    It.Is<BudgetEntry>(a =>
                        a.Amount == command.Amount && a.Description == command.Description
                    ),
                    cancellationToken
                )
            )
            .ReturnsAsync((BudgetEntry newBudgetEntry, CancellationToken _) => newBudgetEntry);
        accountRepo
            .Setup(repo => repo.GetAccount(It.IsAny<Guid>(), cancellationToken))
            .ReturnsAsync(account);
        accountRepo
            .Setup(repo => repo.GetAccountingPeriod(It.IsAny<Guid>(), cancellationToken))
            .ReturnsAsync(accountingPeriod);

        var handler = new SetBudget.Handler(mockRepo.Object, accountRepo.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        mockRepo.Verify(repo =>
            repo.SetBudget(
                It.Is<BudgetEntry>(entry =>
                    entry.Amount == command.Amount
                    && entry.Description == command.Description
                    && entry.AccountingPeriod.Id == accountingPeriod.Id
                    && entry.Id == result
                ),
                cancellationToken
            )
        );
    }

    [Fact]
    public async Task Handle_EmptyValueDate_ShouldAddAccountWithCurrentDate()
    {
        // Arrange
        var mockRepo = new Mock<IBudgetRepository>();
        var accountRepo = new Mock<IAccountRepository>();
        var command = new SmartFaker<SetBudget.Query>().Generate();
        var cancellationToken = new CancellationToken();
        var account = Account.Create("Account", AccountType.Expense, new AccountIcon("favorite", "blue"));
        var accountingPeriod = AccountingPeriod.Create("Test Period", default, default);

        mockRepo
            .Setup(repo =>
                repo.SetBudget(
                    It.Is<BudgetEntry>(a =>
                        a.Amount == command.Amount && a.Description == command.Description
                    ),
                    cancellationToken
                )
            )
            .ReturnsAsync((BudgetEntry newBudgetEntry, CancellationToken _) => newBudgetEntry);
        accountRepo
            .Setup(repo => repo.GetAccount(It.IsAny<Guid>(), cancellationToken))
            .ReturnsAsync(account);
        accountRepo
            .Setup(repo => repo.GetAccountingPeriod(It.IsAny<Guid>(), cancellationToken))
            .ReturnsAsync(accountingPeriod);

        var handler = new SetBudget.Handler(mockRepo.Object, accountRepo.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        mockRepo.Verify(repo =>
            repo.SetBudget(
                It.Is<BudgetEntry>(entry =>
                    entry.Amount == command.Amount
                    && entry.Description == command.Description
                    && entry.Id == result
                    && entry.AccountingPeriod.Id == accountingPeriod.Id
                ),
                cancellationToken
            )
        );
    }
}
