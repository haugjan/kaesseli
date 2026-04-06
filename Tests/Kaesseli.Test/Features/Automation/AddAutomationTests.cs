using Kaesseli.Features.Accounts;
using Kaesseli.Features.Automation;
using Kaesseli.Features.Integration.NextOpenTransaction;
using Kaesseli.Test.Faker;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Kaesseli.Test.Features.Automation;

public class AddAutomationTests
{
    private readonly IAutomationRepository _repoMock = Substitute.For<IAutomationRepository>();
    private readonly IAccountRepository _accountRepoMock = Substitute.For<IAccountRepository>();
    private readonly ApplyAllAutomations.IHandler _applyMock = Substitute.For<ApplyAllAutomations.IHandler>();
    private readonly AddAutomation.Handler _handler;

    public AddAutomationTests() =>
        _handler = new AddAutomation.Handler(_repoMock, _accountRepoMock, _applyMock);

    [Fact]
    public async Task Handle_CreatesAutomationAndApplies()
    {
        var account = new SmartFaker<Account>().Generate();
        var entries = new[] { new SplitOpenTransactionEntry(account.Id, 100m) };
        var query = new AddAutomation.Query("MIGROS*", Guid.NewGuid(), entries);

        _accountRepoMock.GetAccount(account.Id, Arg.Any<CancellationToken>())
            .Returns(account);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.ShouldNotBe(Guid.Empty);
        await _repoMock.Received(1).AddAutomation(
            Arg.Is<AutomationEntry>(a => a.AutomationText == "MIGROS*"),
            Arg.Any<CancellationToken>());
        await _applyMock.Received(1).Handle(
            Arg.Is<ApplyAllAutomations.Query>(q => q.AccountingPeriodId == query.AccountingPeriodId),
            Arg.Any<CancellationToken>());
    }
}
