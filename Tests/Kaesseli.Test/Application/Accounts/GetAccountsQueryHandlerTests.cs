using FluentAssertions;
using Kaesseli.Application.Accounts;
using Kaesseli.Domain.Accounts;
using Kaesseli.Test.Faker;
using Moq;
using Xunit;

namespace Kaesseli.Test.Application.Accounts;

public class GetAccountsQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsCorrectAccounts()
    {
        // Arrange
        var mockRepository = new Mock<IAccountRepository>();
        var faker = new SmartFaker<Account>().RuleFor(a => a.Type, _ => AccountType.Asset);
        var cancellationToken = new CancellationToken();

        var accountsList = faker.Generate(count: 5);
        mockRepository
            .Setup(repo => repo.GetAccounts(cancellationToken))
            .ReturnsAsync(accountsList);

        var handler = new GetAccounts.Handler(mockRepository.Object);
        var query = new GetAccounts.Query();

        // Act
        var result = (await handler.Handle(query, cancellationToken)).ToArray();

        // Assert
        result.Should().BeEquivalentTo(accountsList, options => options.Excluding(al => al.Type));
        result
            .Select(r => r.Type)
            .Should()
            .BeEquivalentTo(expectation: accountsList.Select(a => a.Type.DisplayName()));
        mockRepository.Verify(repo => repo.GetAccounts(cancellationToken), Times.Once);
    }
}
