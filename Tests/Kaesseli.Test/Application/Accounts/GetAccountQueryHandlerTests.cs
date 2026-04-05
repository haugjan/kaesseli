using Kaesseli.Features.Accounts;
using Kaesseli.Infrastructure;
using Kaesseli.Features.Budget;
using Kaesseli.Features.Journal;
using Kaesseli.Test.Faker;
using Moq;
using Shouldly;
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
        var periodId = Guid.NewGuid();
        mockAccountRepo
            .Setup(repo =>
                repo.GetAccount(It.Is<Guid>(guid => guid == expectedAccount.Id), cancellationToken)
            )
            .ReturnsAsync(expectedAccount);
        mockAccountRepo
            .Setup(repo => repo.GetAccountingPeriod(periodId, cancellationToken))
            .ReturnsAsync(new AccountingPeriod
            {
                Id = periodId,
                Description = string.Empty,
                FromInclusive = new DateOnly(year: 2023, month: 1, day: 1),
                ToInclusive = new DateOnly(year: 2024, month: 1, day: 1),
            });
        mockJournalRepo
            .Setup(repo => repo.GetJournalEntries(
                periodId, expectedAccount.Id, null, cancellationToken))
            .ReturnsAsync(Array.Empty<JournalEntry>());
        mockBudgetRepo
            .Setup(repo => repo.GetBudgetEntries(
                periodId, expectedAccount.Id, null, cancellationToken))
            .ReturnsAsync(Array.Empty<BudgetEntry>());

        var handler = new GetAccount.Handler(
            mockAccountRepo.Object,
            mockJournalRepo.Object,
            mockBudgetRepo.Object,
            mockDateTimeService.Object
        );
        var query = new GetAccount.Query(AccountId: expectedAccount.Id, AccountingPeriodId: periodId);

        // Act
        var result = await handler.Handle(query, cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(expectedAccount.Id);
        result.Name.ShouldBe(expectedAccount.Name);
        result.TypeId.ShouldBe(expectedAccount.Type);
        result.Icon.ShouldBe(expectedAccount.Icon.Name);
        result.IconColor.ShouldBe(expectedAccount.Icon.Color);
        mockAccountRepo.Verify(
            repo =>
                repo.GetAccount(It.Is<Guid>(guid => guid == expectedAccount.Id), cancellationToken),
            Times.Once
        );
    }
}
