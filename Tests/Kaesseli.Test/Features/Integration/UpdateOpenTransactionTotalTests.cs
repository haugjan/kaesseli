using Kaesseli.Features.Integration;
using Kaesseli.Features.Integration.NextOpenTransaction;
using NSubstitute;
using Xunit;

namespace Kaesseli.Test.Features.Integration;

public class UpdateOpenTransactionTotalTests
{
    [Fact]
    public async Task Handle_DelegatesToRepository()
    {
        var repoMock = Substitute.For<ITransactionRepository>();
        var handler = new UpdateOpenTransactionTotal.Handler(repoMock);

        await handler.Handle(new UpdateOpenTransactionTotal.Query(3), CancellationToken.None);

        await repoMock.Received(1).ChangeTotalOpenTransaction(3, Arg.Any<CancellationToken>());
    }
}
