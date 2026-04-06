using Kaesseli.Features.Accounts;
using Kaesseli.Features.Budget;
using Kaesseli.Features.Journal;
using Kaesseli.Test.Faker;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Kaesseli.Test.Features.Accounts;

public class GetAccountQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsCorrectAccounts()
    {
        // Arrange
        var mockAccountRepo = Substitute.For<IAccountRepository>();
        var mockJournalRepo = Substitute.For<IJournalRepository>();
        var mockBudgetRepo = Substitute.For<IBudgetRepository>();

        var faker = new SmartFaker<Account>().RuleFor(a => a.Type, _ => AccountType.Asset);
        var cancellationToken = new CancellationToken();

        var expectedAccount = faker.Generate();
        var periodId = Guid.NewGuid();
        mockAccountRepo
            .GetAccount(Arg.Is<Guid>(guid => guid == expectedAccount.Id), cancellationToken)
            .Returns(expectedAccount);
        var accountingPeriod = AccountingPeriod.Create(
            "Test Period",
            new DateOnly(year: 2023, month: 1, day: 1),
            new DateOnly(year: 2024, month: 1, day: 1)
        );
        mockAccountRepo
            .GetAccountingPeriod(periodId, cancellationToken)
            .Returns(accountingPeriod);
        mockJournalRepo
            .GetJournalEntries(
                periodId, expectedAccount.Id, null, cancellationToken)
            .Returns(Array.Empty<JournalEntry>());
        mockBudgetRepo
            .GetBudgetEntries(
                periodId, expectedAccount.Id, null, cancellationToken)
            .Returns(Array.Empty<BudgetEntry>());

        var handler = new GetAccount.Handler(
            mockAccountRepo,
            mockJournalRepo,
            mockBudgetRepo,
            TimeProvider.System
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
        await mockAccountRepo.Received(1)
            .GetAccount(Arg.Is<Guid>(guid => guid == expectedAccount.Id), cancellationToken);
    }
}
