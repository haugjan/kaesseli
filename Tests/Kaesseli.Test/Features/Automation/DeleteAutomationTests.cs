using Kaesseli.Features.Automation;
using NSubstitute;
using Xunit;

namespace Kaesseli.Test.Features.Automation;

public class DeleteAutomationTests
{
    private readonly IAutomationRepository _repoMock = Substitute.For<IAutomationRepository>();
    private readonly DeleteAutomation.Handler _handler;

    public DeleteAutomationTests() => _handler = new DeleteAutomation.Handler(_repoMock);

    [Fact]
    public async Task Handle_DeletesAutomation()
    {
        var id = Guid.NewGuid();

        await _handler.Handle(new DeleteAutomation.Query(id), CancellationToken.None);

        await _repoMock.Received(1).DeleteAutomation(id, Arg.Any<CancellationToken>());
    }
}
