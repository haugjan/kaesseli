using Kaesseli.Features.Integration;
using Kaesseli.Features.Integration.NextOpenTransaction;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Kaesseli.Test.Features.Integration;

public class GetTotalOpenTransactionTests
{
    [Fact]
    public async Task Handle_ReturnsCountFromRepository()
    {
        var repoMock = Substitute.For<ITransactionRepository>();
        repoMock.GetTotalOpenTransaction(Arg.Any<CancellationToken>()).Returns(42);

        var handler = new GetTotalOpenTransaction.Handler(repoMock);
        var result = await handler.Handle(CancellationToken.None);

        result.ShouldBe(42);
    }
}
