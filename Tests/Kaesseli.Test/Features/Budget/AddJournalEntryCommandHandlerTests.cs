using Kaesseli.Features.Accounts;
using Kaesseli.Features.Budget;
using Kaesseli.Test.Faker;
using NSubstitute;
using Xunit;

namespace Kaesseli.Test.Features.Budget;

public class AddJournalEntryCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldAddAccountSuccessfully()
    {
        // Arrange
        var mockRepo = Substitute.For<IBudgetRepository>();
        var accountRepo = Substitute.For<IAccountRepository>();
        var command = new SmartFaker<SetBudget.Query>().Generate();
        var cancellationToken = new CancellationToken();
        var account = AccountFactory.Create(
            "Account",
            AccountType.Expense,
            new AccountIcon("favorite", "blue")
        );
        var accountingPeriod = AccountingPeriod.Create("Test Period", default, default);

        mockRepo
            .SetBudget(
                Arg.Is<BudgetEntry>(a =>
                    a.Amount == command.Amount && a.Description == command.Description
                ),
                cancellationToken
            )
            .Returns(callInfo => callInfo.ArgAt<BudgetEntry>(0));
        accountRepo.GetAccount(Arg.Any<Guid>(), cancellationToken).Returns(account);
        accountRepo
            .GetAccountingPeriod(Arg.Any<Guid>(), cancellationToken)
            .Returns(accountingPeriod);

        var handler = new SetBudget.Handler(mockRepo, accountRepo);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        await mockRepo
            .Received()
            .SetBudget(
                Arg.Is<BudgetEntry>(entry =>
                    entry.Amount == command.Amount
                    && entry.Description == command.Description
                    && entry.AccountingPeriod.Id == accountingPeriod.Id
                    && entry.Id == result
                ),
                cancellationToken
            );
    }

    [Fact]
    public async Task Handle_EmptyValueDate_ShouldAddAccountWithCurrentDate()
    {
        // Arrange
        var mockRepo = Substitute.For<IBudgetRepository>();
        var accountRepo = Substitute.For<IAccountRepository>();
        var command = new SmartFaker<SetBudget.Query>().Generate();
        var cancellationToken = new CancellationToken();
        var account = AccountFactory.Create(
            "Account",
            AccountType.Expense,
            new AccountIcon("favorite", "blue")
        );
        var accountingPeriod = AccountingPeriod.Create("Test Period", default, default);

        mockRepo
            .SetBudget(
                Arg.Is<BudgetEntry>(a =>
                    a.Amount == command.Amount && a.Description == command.Description
                ),
                cancellationToken
            )
            .Returns(callInfo => callInfo.ArgAt<BudgetEntry>(0));
        accountRepo.GetAccount(Arg.Any<Guid>(), cancellationToken).Returns(account);
        accountRepo
            .GetAccountingPeriod(Arg.Any<Guid>(), cancellationToken)
            .Returns(accountingPeriod);

        var handler = new SetBudget.Handler(mockRepo, accountRepo);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        await mockRepo
            .Received()
            .SetBudget(
                Arg.Is<BudgetEntry>(entry =>
                    entry.Amount == command.Amount
                    && entry.Description == command.Description
                    && entry.Id == result
                    && entry.AccountingPeriod.Id == accountingPeriod.Id
                ),
                cancellationToken
            );
    }
}
