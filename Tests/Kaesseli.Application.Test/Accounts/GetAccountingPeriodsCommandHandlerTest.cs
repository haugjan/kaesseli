using FluentAssertions;
using Kaesseli.Application.Accounts;
using Kaesseli.Domain.Accounts;
using Kaesseli.TestUtilities.Faker;
using Moq;
using Xunit;

namespace Kaesseli.Application.Test.Accounts;

public class GetAccountingPeriodsCommandHandlerTest
{
    [Fact]
    public async Task Handle_ReturnsAllAccountingPeriods()
    {
        //Arrange
        var accountRepoMock = new Mock<IAccountRepository>();
        var handler = new GetAccountingPeriodsQueryHandler(accountRepoMock.Object);
        var command = new GetAccountingPeriodsQuery();
        var cancellationToken = new CancellationToken();
        var expectedPeriods = new SmartFaker<AccountingPeriod>().Generate(count: 3);
        accountRepoMock.Setup(repo => repo.GetAccountingPeriods(cancellationToken))
                       .ReturnsAsync(expectedPeriods);

        //Act
        var result = await handler.Handle(command, cancellationToken);

        //Assert
        result.Should().BeEquivalentTo(expectedPeriods);
    }
}