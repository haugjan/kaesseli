using Kaesseli.Features.Accounts;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Kaesseli.Test.Features.Accounts;

public class AddAccountingPeriodHandlerTests
{
    [Fact]
    public async Task Handle_WithDescription_UsesProvidedDescription()
    {
        var repoMock = Substitute.For<IAccountRepository>();
        repoMock.AddAccountingPeriod(Arg.Any<AccountingPeriod>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.ArgAt<AccountingPeriod>(0));

        var handler = new AddAccountingPeriod.Handler(repoMock);
        var query = new AddAccountingPeriod.Query(new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), "Jahr 2026");

        var result = await handler.Handle(query, CancellationToken.None);

        result.ShouldNotBe(Guid.Empty);
        await repoMock.Received(1).AddAccountingPeriod(
            Arg.Is<AccountingPeriod>(ap => ap.Description == "Jahr 2026"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithoutDescription_GeneratesDateRange()
    {
        var repoMock = Substitute.For<IAccountRepository>();
        repoMock.AddAccountingPeriod(Arg.Any<AccountingPeriod>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.ArgAt<AccountingPeriod>(0));

        var handler = new AddAccountingPeriod.Handler(repoMock);
        var query = new AddAccountingPeriod.Query(new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), null);

        await handler.Handle(query, CancellationToken.None);

        await repoMock.Received(1).AddAccountingPeriod(
            Arg.Is<AccountingPeriod>(ap => ap.Description.Contains("2026")),
            Arg.Any<CancellationToken>());
    }
}
