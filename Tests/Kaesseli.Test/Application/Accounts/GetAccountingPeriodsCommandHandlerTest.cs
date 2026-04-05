using Kaesseli.Features.Accounts;
using Kaesseli.Test.Faker;
using Moq;
using Xunit;

namespace Kaesseli.Test.Application.Accounts;

public class GetAccountingPeriodsCommandHandlerTest
{
    [Fact]
    public async Task Handle_ReturnsAllAccountingPeriods()
    {
        //Arrange
        var accountRepoMock = new Mock<IAccountRepository>();
        var handler = new GetAccountingPeriods.Handler(accountRepoMock.Object);
        var cancellationToken = new CancellationToken();
        var expectedPeriods = new SmartFaker<AccountingPeriod>().Generate(count: 3);
        accountRepoMock
            .Setup(repo => repo.GetAccountingPeriods(cancellationToken))
            .ReturnsAsync(expectedPeriods);

        //Act
        var result = await handler.Handle(cancellationToken);

        //Assert
        Assert.Equivalent(expectedPeriods, result);
    }
}
