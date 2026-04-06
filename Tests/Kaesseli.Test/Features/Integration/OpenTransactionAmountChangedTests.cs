using Kaesseli.Features.Integration;
using Kaesseli.Features.Integration.NextOpenTransaction;
using NSubstitute;
using Xunit;

namespace Kaesseli.Test.Features.Integration;

public class OpenTransactionAmountChangedTests
{
    [Fact]
    public async Task Handle_DelegatesToRepository()
    {
        var repoMock = Substitute.For<ITransactionRepository>();
        var handler = new OpenTransactionAmountChanged.Handler(repoMock);

        await handler.Handle(new OpenTransactionAmountChanged.Event(3), CancellationToken.None);

        await repoMock.Received(1).ChangeTotalOpenTransaction(3, Arg.Any<CancellationToken>());
    }
}
