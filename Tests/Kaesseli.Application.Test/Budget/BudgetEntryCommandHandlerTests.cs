using FluentAssertions;
using Kaesseli.Application.Budget;
using Kaesseli.Domain.Entities;
using Kaesseli.Domain.Repositories;
using Moq;
using Xunit;

namespace Kaesseli.Application.Test.Budget;

public class BudgetEntryCommandHandlerTests
{
    private readonly Mock<IBudgetRepository> _mockBudgetRepository;
    private readonly BudgetEntryCommandHandler _handler;

    public BudgetEntryCommandHandlerTests()
    {
        _mockBudgetRepository = new Mock<IBudgetRepository>();
        _handler = new BudgetEntryCommandHandler(_mockBudgetRepository.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnGuid_WhenBudgetEntryIsAddedSuccessfully()
    {
        // Arrange
        var command = new AddBudgetEntryCommand
        {
            AccountId = Guid.NewGuid()
        };
        var fakeBudgetEntry = new BudgetEntry
        {
            Id = Guid.NewGuid()
        };

        _mockBudgetRepository.Setup(repo => repo.AddBudgetEntry(It.IsAny<BudgetEntry>()))
            .ReturnsAsync(fakeBudgetEntry);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(fakeBudgetEntry.Id);
    }
    [Fact]
    public async Task Handle_ShouldCallAddBudgetEntryOnRepository()
    {
        // Arrange
        var command = new AddBudgetEntryCommand
        {
            AccountId = Guid.NewGuid()
        };
        var fakeBudgetEntry = new BudgetEntry
        {
            Id = Guid.NewGuid()
        };

        _mockBudgetRepository.Setup(repo => repo.AddBudgetEntry(It.IsAny<BudgetEntry>()))
            .ReturnsAsync(fakeBudgetEntry);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockBudgetRepository.Verify(repo => repo.AddBudgetEntry(It.IsAny<BudgetEntry>()), Times.Once());
    }
}