using Kaesseli.Application.Budget;
using Kaesseli.Domain.Accounts;
using Kaesseli.Domain.Budget;
using Kaesseli.TestUtilities.Faker;
using Moq;
using Xunit;

namespace Kaesseli.Application.Test.Budget;

public class AddJournalEntryCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldAddAccountSuccessfully()
    {
        // Arrange
        var mockRepo = new Mock<IBudgetRepository>();
        var accountRepo = new Mock<IAccountRepository>();
        var command = new SmartFaker<SetBudgetCommand>().Generate();
        var cancellationToken = new CancellationToken();

        mockRepo.Setup(
                    repo => repo.SetBudget(
                        It.Is<BudgetEntry>(a => a.Amount == command.Amount && a.Description == command.Description),
                        cancellationToken))
                .ReturnsAsync((BudgetEntry newBudgetEntry, CancellationToken _) => newBudgetEntry);
        accountRepo.Setup(repo => repo.GetAccount(It.IsAny<Guid>(), cancellationToken))
                   .ReturnsAsync(() => new Account
                   {
                       Id = Guid.NewGuid(),
                       Name = "Account",
                       Type = AccountType.Expense,
                       Icon = new AccountIcon("favorite", "blue")
                   });
        accountRepo.Setup(repo => repo.GetAccountingPeriod(It.IsAny<Guid>(), cancellationToken))
                   .ReturnsAsync(
                       (Guid accountingPeriodId, CancellationToken _) =>
                           new AccountingPeriod
                           {
                               Id = accountingPeriodId,
                               FromInclusive = default,
                               ToInclusive = default,
                               Description = string.Empty
                           });

        var handler = new SetBudgetCommandHandler(mockRepo.Object, accountRepo.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        mockRepo.Verify(
            repo => repo.SetBudget(
                It.Is<BudgetEntry>(
                    entry => entry.Amount == command.Amount
                          && entry.Description == command.Description
                          && entry.AccountingPeriod.Id == command.AccountingPeriodId
                          && entry.Id == result),
                cancellationToken));
    }

    [Fact]
    public async Task Handle_EmptyValueDate_ShouldAddAccountWithCurrentDate()
    {
        // Arrange
        var mockRepo = new Mock<IBudgetRepository>();
        var accountRepo = new Mock<IAccountRepository>();
        var command = new SmartFaker<SetBudgetCommand>().Generate();
        var cancellationToken = new CancellationToken();

        mockRepo.Setup(
                    repo => repo.SetBudget(
                        It.Is<BudgetEntry>(a => a.Amount == command.Amount && a.Description == command.Description),
                        cancellationToken))
                .ReturnsAsync((BudgetEntry newBudgetEntry, CancellationToken _) => newBudgetEntry);
        accountRepo.Setup(repo => repo.GetAccount(It.IsAny<Guid>(), cancellationToken))
                   .ReturnsAsync(() => new Account
                   {
                       Id = Guid.NewGuid(),
                       Name = "Account",
                       Type = AccountType.Expense,
                       Icon = new AccountIcon("favorite", "blue")
                   });
        accountRepo.Setup(repo => repo.GetAccountingPeriod(It.IsAny<Guid>(), cancellationToken))
                   .ReturnsAsync(
                       (Guid accountingPeriodId, CancellationToken _) =>
                           new AccountingPeriod
                           {
                               Id = accountingPeriodId,
                               FromInclusive = default,
                               ToInclusive = default,
                               Description = string.Empty
                           });


        var handler = new SetBudgetCommandHandler(mockRepo.Object, accountRepo.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        mockRepo.Verify(
            repo => repo.SetBudget(
                It.Is<BudgetEntry>(
                    entry => entry.Amount == command.Amount
                          && entry.Description == command.Description
                          && entry.Id == result
                             && entry.AccountingPeriod.Id == command.AccountingPeriodId),
                cancellationToken));
    }
}