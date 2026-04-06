using Kaesseli.Features.Integration;
using Kaesseli.Features.Integration.NextOpenTransaction;
using Moq;
using Shouldly;
using Xunit;

namespace Kaesseli.Test.Features.Integration;

public class GetTotalOpenTransactionTests
{
    [Fact]
    public async Task Handle_ReturnsCountFromRepository()
    {
        var repoMock = new Mock<ITransactionRepository>();
        repoMock.Setup(x => x.GetTotalOpenTransaction(It.IsAny<CancellationToken>())).ReturnsAsync(42);

        var handler = new GetTotalOpenTransaction.Handler(repoMock.Object);
        var result = await handler.Handle(CancellationToken.None);

        result.ShouldBe(42);
    }
}
