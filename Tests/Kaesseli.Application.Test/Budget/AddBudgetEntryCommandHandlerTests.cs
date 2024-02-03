using Bogus;
using FluentAssertions;
using Kaesseli.Application.Budget;
using Kaesseli.Application.Common;
using Kaesseli.Domain.Accounts;
using Kaesseli.Domain.Budget;
using Kaesseli.TestUtilities.Faker;
using Moq;
using Xunit;

namespace Kaesseli.Application.Test.Budget;

public class AddBudgetEntryCommandHandlerTests
{
    private readonly Mock<IBudgetRepository> _mockBudgetRepository;
    private readonly AddBudgetEntryCommandHandler _handler;

    public AddBudgetEntryCommandHandlerTests()
    {
        _mockBudgetRepository = new Mock<IBudgetRepository>();
        var mockAccountRepository = new Mock<IAccountRepository>();
        var mockDateTimeService = new Mock<IDateTimeService>();
        _handler = new AddBudgetEntryCommandHandler(
            _mockBudgetRepository.Object,
            mockAccountRepository.Object,
            mockDateTimeService.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnGuid_WhenBudgetEntryIsAddedSuccessfully()
    {
        // Arrange
        var command = new Faker<AddBudgetEntryCommand>().UseSeed(seed: 0).Generate();
        var fakeBudgetEntry = new SmartFaker<BudgetEntry>().Generate();
        var cancellationToken = new CancellationToken();

        _mockBudgetRepository.Setup(repo => repo.AddBudgetEntry(It.IsAny<BudgetEntry>(), cancellationToken))
                             .ReturnsAsync(fakeBudgetEntry);

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        result.Should().Be(fakeBudgetEntry.Id);
    }

    [Fact]
    public async Task Handle_ShouldCallAddBudgetEntryOnRepository()
    {
        // Arrange
        var command = new Faker<AddBudgetEntryCommand>().UseSeed(seed: 0).Generate();
        var fakeBudgetEntry = new Faker<BudgetEntry>().UseSeed(seed: 1).Generate();
        var cancellationToken = new CancellationToken();

        _mockBudgetRepository.Setup(repo => repo.AddBudgetEntry(It.IsAny<BudgetEntry>(), cancellationToken))
                             .ReturnsAsync(fakeBudgetEntry);

        // Act
        await _handler.Handle(command, cancellationToken);

        // Assert
        _mockBudgetRepository.Verify(repo => repo.AddBudgetEntry(It.IsAny<BudgetEntry>(), cancellationToken), times: Times.Once());
    }
}