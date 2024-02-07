using Bogus;
using FluentAssertions;
using Kaesseli.Application.Accounts;
using Kaesseli.Domain.Accounts;
using Kaesseli.Domain.Common;
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
        var faker = new Faker<Account>()
                    .RuleFor(a => a.Id, f => f.Random.Guid())
                    .RuleFor(a => a.Name, f => f.Person.FullName);

        var accountsList = faker.Generate(count: 5);
        mockRepository.Setup(repo => repo.GetAccounts(It.IsAny<CancellationToken>()))
                      .ReturnsAsync(accountsList);

        var handler = new GetAccountsQueryHandler(mockRepository.Object);
        var query = new GetAccountsQuery();

        // Act
        var result = (await handler.Handle(query, CancellationToken.None)).ToArray();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(accountsList.Count);
        result.Select(r => r.Name).Should().BeEquivalentTo(expectation: accountsList.Select(a => a.Name));

        mockRepository.Verify(repo => repo.GetAccounts(It.IsAny<CancellationToken>()), Times.Once);
    }
}