using Kaesseli.Features.Automation;
using Moq;
using Shouldly;
using Xunit;

namespace Kaesseli.Test.Features.Automation;

public class GetNrOfPossibleAutomationTests
{
    [Fact]
    public async Task Handle_ReturnCountFromRepository()
    {
        var repoMock = new Mock<IAutomationRepository>();
        repoMock.Setup(x => x.GetNrOfPossibleAutomation("MIGROS*", It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        var handler = new GetNrOfPossibleAutomation.Handler(repoMock.Object);
        var result = await handler.Handle(new GetNrOfPossibleAutomation.Query("MIGROS*"), CancellationToken.None);

        result.NrOfPossibleAutomation.ShouldBe(5);
    }
}
