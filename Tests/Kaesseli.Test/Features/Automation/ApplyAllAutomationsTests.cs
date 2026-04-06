using Kaesseli.Features.Automation;
using Kaesseli.Features.Integration;
using Kaesseli.Features.Integration.NextOpenTransaction;
using Kaesseli.Test.Faker;
using NSubstitute;
using Xunit;

namespace Kaesseli.Test.Features.Automation;

public class ApplyAllAutomationsTests
{
    private readonly IAutomationRepository _repoMock = Substitute.For<IAutomationRepository>();
    private readonly SplitOpenTransaction.IHandler _splitMock = Substitute.For<SplitOpenTransaction.IHandler>();
    private readonly ApplyAllAutomations.Handler _handler;

    public ApplyAllAutomationsTests() =>
        _handler = new ApplyAllAutomations.Handler(_repoMock, _splitMock);

    [Fact]
    public async Task Handle_NoAutomations_DoesNothing()
    {
        _repoMock.GetAutomations(Arg.Any<CancellationToken>())
            .Returns(Array.Empty<AutomationEntry>());

        await _handler.Handle(new ApplyAllAutomations.Query(Guid.NewGuid()), CancellationToken.None);

        await _splitMock.DidNotReceive().Handle(Arg.Any<SplitOpenTransaction.Query>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithMatchingTransactions_CallsSplitForEach()
    {
        var account = new SmartFaker<Kaesseli.Features.Accounts.Account>().Generate();
        var part = AutomationEntryPart.Create(account, 1.0m);
        var automation = AutomationEntry.Create("MIGROS*", [part]);
        var transactions = new SmartFaker<Transaction>().Generate(count: 2);

        _repoMock.GetAutomations(Arg.Any<CancellationToken>())
            .Returns(new[] { automation });
        _repoMock.GetPossibleTransactions("MIGROS*", Arg.Any<CancellationToken>())
            .Returns(transactions);

        await _handler.Handle(new ApplyAllAutomations.Query(Guid.NewGuid()), CancellationToken.None);

        await _splitMock.Received(2).Handle(Arg.Any<SplitOpenTransaction.Query>(), Arg.Any<CancellationToken>());
    }
}
