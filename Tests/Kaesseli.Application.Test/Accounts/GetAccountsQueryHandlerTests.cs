using FluentAssertions;
using Kaesseli.Application.Accounts;
using Kaesseli.Domain.Accounts;
using Kaesseli.TestUtilities.Faker;
using Moq;
using Xunit;

namespace Kaesseli.Application.Test.Accounts;

public class GetAccountsQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsCorrectAccounts()
    {
        // Arrange
        var mockRepository = new Mock<IAccountRepository>();
        var faker = new SmartFaker<Account>()
                    .RuleFor(a => a.Type, _ => AccountType.Asset);
        var cancellationToken = new CancellationToken();

        var accountsList = faker.Generate(count: 5);
        mockRepository.Setup(repo => repo.GetAccounts(cancellationToken))
                      .ReturnsAsync(accountsList);

        var handler = new GetAccountsQueryHandler(mockRepository.Object);
        var query = new GetAccountsQuery();

        // Act
        var result = (await handler.Handle(query, cancellationToken)).ToArray();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(accountsList.Count);
        result.Select(r => r.Id).Should().BeEquivalentTo(expectation: accountsList.Select(a => a.Id));
        result.Select(r => r.Name).Should().BeEquivalentTo(expectation: accountsList.Select(a => a.Name));
        result.Select(r => r.Type).Should().BeEquivalentTo(expectation: accountsList.Select(a => a.Type.DisplayName()));

        mockRepository.Verify(repo => repo.GetAccounts(cancellationToken), Times.Once);
    }
}