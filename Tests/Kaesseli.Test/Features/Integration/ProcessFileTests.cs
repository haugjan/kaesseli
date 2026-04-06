using Kaesseli.Features.Automation;
using Kaesseli.Features.Integration.FileImport;
using Moq;
using Shouldly;
using Xunit;

namespace Kaesseli.Test.Features.Integration;

public class ProcessFileTests
{
    private readonly Mock<ProcessCamtFile.IHandler> _camtMock = new();
    private readonly Mock<ProcessPostFinanceCsv.IHandler> _csvMock = new();
    private readonly Mock<ApplyAllAutomations.IHandler> _automationMock = new();
    private readonly ProcessFile.Handler _handler;

    public ProcessFileTests() =>
        _handler = new ProcessFile.Handler(_camtMock.Object, _csvMock.Object, _automationMock.Object);

    [Fact]
    public async Task Handle_CamtFile_DelegatesToCamtHandler()
    {
        var expectedGuid = Guid.NewGuid();
        _camtMock.Setup(x => x.Handle(It.IsAny<ProcessCamtFile.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedGuid);

        var query = new ProcessFile.Query(FileType.Camt, Stream.Null, Guid.NewGuid(), Guid.NewGuid());
        var result = await _handler.Handle(query, CancellationToken.None);

        result.ShouldBe(expectedGuid);
        _camtMock.Verify(x => x.Handle(It.IsAny<ProcessCamtFile.Query>(), It.IsAny<CancellationToken>()), Times.Once);
        _csvMock.Verify(x => x.Handle(It.IsAny<ProcessPostFinanceCsv.Query>(), It.IsAny<CancellationToken>()), Times.Never);
        _automationMock.Verify(x => x.Handle(It.IsAny<ApplyAllAutomations.Query>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CsvFile_DelegatesToCsvHandler()
    {
        var expectedGuid = Guid.NewGuid();
        _csvMock.Setup(x => x.Handle(It.IsAny<ProcessPostFinanceCsv.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedGuid);

        var query = new ProcessFile.Query(FileType.PostFinanceCsv, Stream.Null, Guid.NewGuid(), Guid.NewGuid());
        var result = await _handler.Handle(query, CancellationToken.None);

        result.ShouldBe(expectedGuid);
        _csvMock.Verify(x => x.Handle(It.IsAny<ProcessPostFinanceCsv.Query>(), It.IsAny<CancellationToken>()), Times.Once);
        _camtMock.Verify(x => x.Handle(It.IsAny<ProcessCamtFile.Query>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_UnknownFileType_Throws()
    {
        var query = new ProcessFile.Query((FileType)99, Stream.Null, Guid.NewGuid(), Guid.NewGuid());
        await Should.ThrowAsync<NotImplementedException>(() => _handler.Handle(query, CancellationToken.None));
    }
}
