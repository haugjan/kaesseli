using Kaesseli.Application.Budget;
using Kaesseli.Application.Utility;
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
        var dateTimeService = new Mock<IDateTimeService>();
        var command = new SmartFaker<AddBudgetEntryCommand>().Generate();
        var cancellationToken = new CancellationToken();

        mockRepo.Setup(
                    repo => repo.AddBudgetEntry(
                        It.Is<BudgetEntry>(a => a.Amount == command.Amount && a.Description == command.Description),
                        cancellationToken))
                .ReturnsAsync((BudgetEntry newBudgetEntry, CancellationToken _) => newBudgetEntry);

        var handler = new AddBudgetEntryCommandHandler(mockRepo.Object, accountRepo.Object, dateTimeService.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        mockRepo.Verify(
            repo => repo.AddBudgetEntry(
                It.Is<BudgetEntry>(
                    entry => entry.Amount == command.Amount
                          && entry.Description == command.Description
                          && entry.ValueDate == command.ValueDate
                          && entry.Id == result),
                cancellationToken));
    }

    [Fact]
    public async Task Handle_EmptyValueDate_ShouldAddAccountWithCurrentDate()
    {
        // Arrange
        var mockRepo = new Mock<IBudgetRepository>();
        var accountRepo = new Mock<IAccountRepository>();
        var dateTimeService = new Mock<IDateTimeService>();
        var command = new SmartFaker<AddBudgetEntryCommand>().RuleFor(c => c.ValueDate, _ => null).Generate();
        var cancellationToken = new CancellationToken();
        var currentDay = new DateOnly(year: 1982, month: 11, day: 3);

        mockRepo.Setup(
                    repo => repo.AddBudgetEntry(
                        It.Is<BudgetEntry>(a => a.Amount == command.Amount && a.Description == command.Description),
                        cancellationToken))
                .ReturnsAsync((BudgetEntry newBudgetEntry, CancellationToken _) => newBudgetEntry);
        dateTimeService.Setup(dts => dts.ToDay).Returns(currentDay);
        var handler = new AddBudgetEntryCommandHandler(mockRepo.Object, accountRepo.Object, dateTimeService.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        mockRepo.Verify(
            repo => repo.AddBudgetEntry(
                It.Is<BudgetEntry>(
                    entry => entry.Amount == command.Amount
                          && entry.Description == command.Description
                          && entry.ValueDate == currentDay
                          && entry.Id == result),
                cancellationToken));
    }
}