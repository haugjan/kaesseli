using FluentAssertions;
using Kaesseli.Application.Accounts;
using Kaesseli.Application.Utility;
using Kaesseli.Domain.Accounts;
using Kaesseli.Domain.Budget;
using Kaesseli.Domain.Journal;
using Kaesseli.Test.Faker;
using Moq;
using Xunit;

namespace Kaesseli.Test.Application.Accounts;

public class GetAccountQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsCorrectAccounts()
    {
        // Arrange
        var mockAccountRepo = new Mock<IAccountRepository>();
        var mockJournalRepo = new Mock<IJournalRepository>();
        var mockBudgetRepo = new Mock<IBudgetRepository>();
        var mockDateTimeService = new Mock<IDateTimeService>();

        var faker = new SmartFaker<Account>().RuleFor(a => a.Type, _ => AccountType.Asset);
        var cancellationToken = new CancellationToken();

        var expectedAccount = faker.Generate();
        mockAccountRepo
            .Setup(repo =>
                repo.GetAccount(It.Is<Guid>(guid => guid == expectedAccount.Id), cancellationToken)
            )
            .ReturnsAsync(expectedAccount);

        var handler = new GetAccount.Handler(
            mockAccountRepo.Object,
            mockJournalRepo.Object,
            mockBudgetRepo.Object,
            mockDateTimeService.Object
        );
        var query = new GetAccount.Query
        {
            AccountId = expectedAccount.Id,
            AccountingPeriodId = Guid.NewGuid(),
        };

        // Act
        var result = await handler.Handle(query, cancellationToken);

        // Assert
        result
            .Should()
            .BeEquivalentTo(expectedAccount, options => options.Excluding(acc => acc.Type));
        mockAccountRepo.Verify(
            repo =>
                repo.GetAccount(It.Is<Guid>(guid => guid == expectedAccount.Id), cancellationToken),
            Times.Once
        );
    }
}
