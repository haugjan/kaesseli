using Kaesseli.Features.Automation;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Kaesseli.Test.Features.Automation;

public class GetNrOfPossibleAutomationTests
{
    [Fact]
    public async Task Handle_ReturnCountFromRepository()
    {
        var repoMock = Substitute.For<IAutomationRepository>();
        repoMock.GetNrOfPossibleAutomation("MIGROS*", Arg.Any<CancellationToken>())
            .Returns(5);

        var handler = new GetNrOfPossibleAutomation.Handler(repoMock);
        var result = await handler.Handle(new GetNrOfPossibleAutomation.Query("MIGROS*"), CancellationToken.None);

        result.NrOfPossibleAutomation.ShouldBe(5);
    }
}
