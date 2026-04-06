using Kaesseli.Features.Automation;
using Kaesseli.Features.Integration.FileImport;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Kaesseli.Test.Features.Integration;

public class ProcessFileTests
{
    private readonly ProcessCamtFile.IHandler _camtMock = Substitute.For<ProcessCamtFile.IHandler>();
    private readonly ProcessPostFinanceCsv.IHandler _csvMock = Substitute.For<ProcessPostFinanceCsv.IHandler>();
    private readonly ApplyAllAutomations.IHandler _automationMock = Substitute.For<ApplyAllAutomations.IHandler>();
    private readonly ProcessFile.Handler _handler;

    public ProcessFileTests() =>
        _handler = new ProcessFile.Handler(_camtMock, _csvMock, _automationMock);

    [Fact]
    public async Task Handle_CamtFile_DelegatesToCamtHandler()
    {
        var expectedGuid = Guid.NewGuid();
        _camtMock.Handle(Arg.Any<ProcessCamtFile.Query>(), Arg.Any<CancellationToken>())
            .Returns(expectedGuid);

        var query = new ProcessFile.Query(FileType.Camt, Stream.Null, Guid.NewGuid(), Guid.NewGuid());
        var result = await _handler.Handle(query, CancellationToken.None);

        result.ShouldBe(expectedGuid);
        await _camtMock.Received(1).Handle(Arg.Any<ProcessCamtFile.Query>(), Arg.Any<CancellationToken>());
        await _csvMock.DidNotReceive().Handle(Arg.Any<ProcessPostFinanceCsv.Query>(), Arg.Any<CancellationToken>());
        await _automationMock.Received(1).Handle(Arg.Any<ApplyAllAutomations.Query>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CsvFile_DelegatesToCsvHandler()
    {
        var expectedGuid = Guid.NewGuid();
        _csvMock.Handle(Arg.Any<ProcessPostFinanceCsv.Query>(), Arg.Any<CancellationToken>())
            .Returns(expectedGuid);

        var query = new ProcessFile.Query(FileType.PostFinanceCsv, Stream.Null, Guid.NewGuid(), Guid.NewGuid());
        var result = await _handler.Handle(query, CancellationToken.None);

        result.ShouldBe(expectedGuid);
        await _csvMock.Received(1).Handle(Arg.Any<ProcessPostFinanceCsv.Query>(), Arg.Any<CancellationToken>());
        await _camtMock.DidNotReceive().Handle(Arg.Any<ProcessCamtFile.Query>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UnknownFileType_Throws()
    {
        var query = new ProcessFile.Query((FileType)99, Stream.Null, Guid.NewGuid(), Guid.NewGuid());
        await Should.ThrowAsync<NotImplementedException>(() => _handler.Handle(query, CancellationToken.None));
    }
}
