using Kaesseli.Features.Accounts;
using Kaesseli.Test.Faker;
using NSubstitute;
using Xunit;

namespace Kaesseli.Test.Features.Accounts;

public class GetAccountingPeriodsCommandHandlerTest
{
    [Fact]
    public async Task Handle_ReturnsAllAccountingPeriods()
    {
        //Arrange
        var accountRepoMock = Substitute.For<IAccountRepository>();
        var handler = new GetAccountingPeriods.Handler(accountRepoMock);
        var cancellationToken = new CancellationToken();
        var expectedPeriods = new SmartFaker<AccountingPeriod>().Generate(count: 3);
        accountRepoMock
            .GetAccountingPeriods(cancellationToken)
            .Returns(expectedPeriods);

        //Act
        var result = await handler.Handle(cancellationToken);

        //Assert
        Assert.Equivalent(expectedPeriods, result);
    }
}
