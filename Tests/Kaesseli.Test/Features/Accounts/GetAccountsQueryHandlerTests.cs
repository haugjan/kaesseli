using Kaesseli.Features.Accounts;
using Kaesseli.Test.Faker;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Kaesseli.Test.Features.Accounts;

public class GetAccountsQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsCorrectAccounts()
    {
        // Arrange
        var mockRepository = Substitute.For<IAccountRepository>();
        var faker = new SmartFaker<Account>().RuleFor(a => a.Type, _ => AccountType.Asset);
        var cancellationToken = new CancellationToken();

        var accountsList = faker.Generate(count: 5);
        mockRepository
            .GetAccounts(cancellationToken)
            .Returns(accountsList);

        var handler = new GetAccounts.Handler(mockRepository);
        var query = new GetAccounts.Query();

        // Act
        var result = (await handler.Handle(query, cancellationToken)).ToArray();

        // Assert
        result.Select(r => r.Id).ToArray().ShouldBeEquivalentTo(accountsList.Select(a => a.Id).ToArray());
        result.Select(r => r.TypeId).ToArray().ShouldBeEquivalentTo(accountsList.Select(a => a.Type).ToArray());
        await mockRepository.Received(1).GetAccounts(cancellationToken);
    }
}
