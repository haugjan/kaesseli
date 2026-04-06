using Kaesseli.Features.Integration;
using Kaesseli.Features.Integration.NextOpenTransaction;
using Moq;
using Xunit;

namespace Kaesseli.Test.Features.Integration;

public class OpenTransactionAmountChangedTests
{
    [Fact]
    public async Task Handle_DelegatesToRepository()
    {
        var repoMock = new Mock<ITransactionRepository>();
        var handler = new OpenTransactionAmountChanged.Handler(repoMock.Object);

        await handler.Handle(new OpenTransactionAmountChanged.Event(3), CancellationToken.None);

        repoMock.Verify(x => x.ChangeTotalOpenTransaction(3, It.IsAny<CancellationToken>()), Times.Once);
    }
}
