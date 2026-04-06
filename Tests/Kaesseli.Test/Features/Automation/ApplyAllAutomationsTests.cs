using Kaesseli.Features.Automation;
using Kaesseli.Features.Integration;
using Kaesseli.Features.Integration.NextOpenTransaction;
using Kaesseli.Test.Faker;
using Moq;
using Xunit;

namespace Kaesseli.Test.Features.Automation;

public class ApplyAllAutomationsTests
{
    private readonly Mock<IAutomationRepository> _repoMock = new();
    private readonly Mock<SplitOpenTransaction.IHandler> _splitMock = new();
    private readonly ApplyAllAutomations.Handler _handler;

    public ApplyAllAutomationsTests() =>
        _handler = new ApplyAllAutomations.Handler(_repoMock.Object, _splitMock.Object);

    [Fact]
    public async Task Handle_NoAutomations_DoesNothing()
    {
        _repoMock.Setup(x => x.GetAutomations(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<AutomationEntry>());

        await _handler.Handle(new ApplyAllAutomations.Query(Guid.NewGuid()), CancellationToken.None);

        _splitMock.Verify(x => x.Handle(It.IsAny<SplitOpenTransaction.Query>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithMatchingTransactions_CallsSplitForEach()
    {
        var account = new SmartFaker<Kaesseli.Features.Accounts.Account>().Generate();
        var part = AutomationEntryPart.Create(account, 1.0m);
        var automation = AutomationEntry.Create("MIGROS*", [part]);
        var transactions = new SmartFaker<Transaction>().Generate(count: 2);

        _repoMock.Setup(x => x.GetAutomations(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { automation });
        _repoMock.Setup(x => x.GetPossibleTransactions("MIGROS*", It.IsAny<CancellationToken>()))
            .ReturnsAsync(transactions);

        await _handler.Handle(new ApplyAllAutomations.Query(Guid.NewGuid()), CancellationToken.None);

        _splitMock.Verify(x => x.Handle(It.IsAny<SplitOpenTransaction.Query>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }
}
