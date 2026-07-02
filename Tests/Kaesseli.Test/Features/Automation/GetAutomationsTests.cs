using Kaesseli.Features.Automation;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Kaesseli.Test.Features.Automation;

public class GetAutomationsTests
{
    private readonly IAutomationRepository _repoMock = Substitute.For<IAutomationRepository>();
    private readonly GetAutomations.Handler _handler;

    public GetAutomationsTests() => _handler = new GetAutomations.Handler(_repoMock);

    [Fact]
    public async Task Handle_MapsDomainToContract()
    {
        var entry = AutomationEntry.Create(
            "MIGROS*",
            [
                AutomationEntryPart.Create("groceries", 0.75m),
                AutomationEntryPart.Create("household", 0.25m),
            ]
        );
        _repoMock.GetAutomations(Arg.Any<CancellationToken>()).Returns([entry]);

        var result = (await _handler.Handle(CancellationToken.None)).ToList();

        result.Count.ShouldBe(1);
        result[0].Id.ShouldBe(entry.Id);
        result[0].AutomationText.ShouldBe("MIGROS*");
        result[0].Parts.Count.ShouldBe(2);
        result[0].Parts[0].AccountShortName.ShouldBe("groceries");
        result[0].Parts[0].AmountProportion.ShouldBe(0.75m);
    }
}
