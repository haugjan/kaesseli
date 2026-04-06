using Kaesseli.Features.Accounts;
using Moq;
using Shouldly;
using Xunit;

namespace Kaesseli.Test.Features.Accounts;

public class AddAccountingPeriodHandlerTests
{
    [Fact]
    public async Task Handle_WithDescription_UsesProvidedDescription()
    {
        var repoMock = new Mock<IAccountRepository>();
        repoMock.Setup(x => x.AddAccountingPeriod(It.IsAny<AccountingPeriod>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AccountingPeriod ap, CancellationToken _) => ap);

        var handler = new AddAccountingPeriod.Handler(repoMock.Object);
        var query = new AddAccountingPeriod.Query(new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), "Jahr 2026");

        var result = await handler.Handle(query, CancellationToken.None);

        result.ShouldNotBe(Guid.Empty);
        repoMock.Verify(x => x.AddAccountingPeriod(
            It.Is<AccountingPeriod>(ap => ap.Description == "Jahr 2026"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithoutDescription_GeneratesDateRange()
    {
        var repoMock = new Mock<IAccountRepository>();
        repoMock.Setup(x => x.AddAccountingPeriod(It.IsAny<AccountingPeriod>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AccountingPeriod ap, CancellationToken _) => ap);

        var handler = new AddAccountingPeriod.Handler(repoMock.Object);
        var query = new AddAccountingPeriod.Query(new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), null);

        await handler.Handle(query, CancellationToken.None);

        repoMock.Verify(x => x.AddAccountingPeriod(
            It.Is<AccountingPeriod>(ap => ap.Description.Contains("2026")),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
