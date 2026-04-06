using Kaesseli.Features.Accounts;
using Kaesseli.Features.Automation;
using Kaesseli.Features.Integration.NextOpenTransaction;
using Kaesseli.Test.Faker;
using Moq;
using Shouldly;
using Xunit;

namespace Kaesseli.Test.Features.Automation;

public class AddAutomationTests
{
    private readonly Mock<IAutomationRepository> _repoMock = new();
    private readonly Mock<IAccountRepository> _accountRepoMock = new();
    private readonly Mock<ApplyAllAutomations.IHandler> _applyMock = new();
    private readonly AddAutomation.Handler _handler;

    public AddAutomationTests() =>
        _handler = new AddAutomation.Handler(_repoMock.Object, _accountRepoMock.Object, _applyMock.Object);

    [Fact]
    public async Task Handle_CreatesAutomationAndApplies()
    {
        var account = new SmartFaker<Account>().Generate();
        var entries = new[] { new SplitOpenTransactionEntry(account.Id, 100m) };
        var query = new AddAutomation.Query("MIGROS*", Guid.NewGuid(), entries);

        _accountRepoMock.Setup(x => x.GetAccount(account.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.ShouldNotBe(Guid.Empty);
        _repoMock.Verify(x => x.AddAutomation(
            It.Is<AutomationEntry>(a => a.AutomationText == "MIGROS*"),
            It.IsAny<CancellationToken>()), Times.Once);
        _applyMock.Verify(x => x.Handle(
            It.Is<ApplyAllAutomations.Query>(q => q.AccountingPeriodId == query.AccountingPeriodId),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
