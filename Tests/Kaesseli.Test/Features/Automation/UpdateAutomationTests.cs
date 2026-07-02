using Kaesseli.Features.Automation;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Kaesseli.Test.Features.Automation;

public class UpdateAutomationTests
{
    private readonly IAutomationRepository _repoMock = Substitute.For<IAutomationRepository>();
    private readonly UpdateAutomation.Handler _handler;

    public UpdateAutomationTests() => _handler = new UpdateAutomation.Handler(_repoMock);

    [Fact]
    public async Task Handle_NormalizesProportionsAndUpdates()
    {
        var id = Guid.NewGuid();
        var query = new UpdateAutomation.Query(
            id,
            "MIGROS*",
            [
                new UpdateAutomation.PartRequest("groceries", 30m),
                new UpdateAutomation.PartRequest("household", 10m),
            ]
        );

        await _handler.Handle(query, CancellationToken.None);

        await _repoMock
            .Received(1)
            .UpdateAutomation(
                id,
                "MIGROS*",
                Arg.Is<IEnumerable<AutomationEntryPart>>(parts =>
                    parts.Count() == 2
                    && parts.First().AccountShortName == "groceries"
                    && parts.First().AmountProportion == 0.75m
                    && parts.Last().AccountShortName == "household"
                    && parts.Last().AmountProportion == 0.25m
                ),
                Arg.Any<CancellationToken>()
            );
    }
}
