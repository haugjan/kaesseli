using Kaesseli.Features.Automation;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Kaesseli.Test.Features.Automation;

public class AddAutomationTests
{
    private readonly IAutomationRepository _repoMock = Substitute.For<IAutomationRepository>();
    private readonly ApplyAllAutomations.IHandler _applyMock =
        Substitute.For<ApplyAllAutomations.IHandler>();
    private readonly AddAutomation.Handler _handler;

    public AddAutomationTests() => _handler = new AddAutomation.Handler(_repoMock, _applyMock);

    [Fact]
    public async Task Handle_CreatesAutomationAndApplies()
    {
        var entries = new[] { new AddAutomation.AutomationEntryRequest("groceries", 100m) };
        var query = new AddAutomation.Query("MIGROS*", Guid.NewGuid(), entries);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.ShouldNotBe(Guid.Empty);
        await _repoMock
            .Received(1)
            .AddAutomation(
                Arg.Is<AutomationEntry>(a =>
                    a.AutomationText == "MIGROS*"
                    && a.Parts.Single().AccountShortName == "groceries"
                    && a.Parts.Single().AmountProportion == 1m
                ),
                Arg.Any<CancellationToken>()
            );
        await _applyMock
            .Received(1)
            .Handle(
                Arg.Is<ApplyAllAutomations.Query>(q =>
                    q.AccountingPeriodId == query.AccountingPeriodId
                ),
                Arg.Any<CancellationToken>()
            );
    }
}
