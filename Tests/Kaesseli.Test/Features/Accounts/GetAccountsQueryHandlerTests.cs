using Kaesseli.Features.Accounts;
using Kaesseli.Test.Faker;
using Moq;
using Shouldly;
using Xunit;

namespace Kaesseli.Test.Features.Accounts;

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
        result.Select(r => r.Id).ToArray().ShouldBeEquivalentTo(accountsList.Select(a => a.Id).ToArray());
        result.Select(r => r.Type).ToArray().ShouldBeEquivalentTo(
            accountsList.Select(a => a.Type.DisplayName()).ToArray()
        );
        mockRepository.Verify(repo => repo.GetAccounts(cancellationToken), Times.Once);
    }
}
