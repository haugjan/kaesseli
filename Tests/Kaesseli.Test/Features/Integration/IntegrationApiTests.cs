using System.Net;
using System.Net.Http.Headers;
using Kaesseli.Contracts.Integration;
using Kaesseli.Features.Integration.FileImport;
using Kaesseli.Features.Integration.NextOpenTransaction;
using Kaesseli.Features.Integration.TransactionQuery;
using Kaesseli.Test.Faker;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shouldly;
using Xunit;

namespace Kaesseli.Test.Features.Integration;

public class IntegrationApiTests : IAsyncLifetime
{
    private HttpClient _client = null!;
    private readonly Mock<ProcessFile.IHandler> _processFileMock = new();
    private readonly Mock<GetTransactionSummaries.IHandler> _getTransactionSummariesMock = new();
    private readonly Mock<GetTransactions.IHandler> _getTransactionsMock = new();
    private readonly Mock<GetNextOpenTransaction.IHandler> _getNextOpenTransactionMock = new();
    private readonly Mock<GetTotalOpenTransaction.IHandler> _getTotalOpenTransactionMock = new();
    private readonly Mock<AssignOpenTransaction.IHandler> _assignOpenTransactionMock = new();
    private readonly Mock<SplitOpenTransaction.IHandler> _splitOpenTransactionMock = new();

    public async Task InitializeAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddRouting();
        builder.Services.AddSingleton(_processFileMock.Object);
        builder.Services.AddSingleton(_getTransactionSummariesMock.Object);
        builder.Services.AddSingleton(_getTransactionsMock.Object);
        builder.Services.AddSingleton(_getNextOpenTransactionMock.Object);
        builder.Services.AddSingleton(_getTotalOpenTransactionMock.Object);
        builder.Services.AddSingleton(_assignOpenTransactionMock.Object);
        builder.Services.AddSingleton(_splitOpenTransactionMock.Object);
        builder.Services.AddAntiforgery();

        var app = builder.Build();
        app.UseAntiforgery();
        app.MapIntegrationEndpoints();

        await app.StartAsync();
        _client = app.GetTestClient();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task CamtUploadEndpoint_ShouldReturnCreatedResult()
    {
        // Arrange
        var guid = Guid.NewGuid();
        _processFileMock
            .Setup(m => m.Handle(It.IsAny<ProcessFile.Query>(), default))
            .ReturnsAsync(guid);

        var formContent = new MultipartFormDataContent();
        var accountId = Guid.NewGuid();
        var fileContent = new ByteArrayContent(content: "Dummy File Content"u8.ToArray());
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(input: "multipart/form-data");
        formContent.Add(fileContent, name: "file", fileName: "dummy_file.camt");

        var accountIdContent = new StringContent(content: accountId.ToString());
        formContent.Add(accountIdContent, name: "accountId");
        var accountingPeriodIdContent = new StringContent(content: Guid.NewGuid().ToString());
        formContent.Add(accountingPeriodIdContent, name: "accountingPeriodId");
        // Act
        var response = await _client.PostAsync(requestUri: "/file/upload", formContent);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        _processFileMock.Verify(m => m.Handle(It.IsAny<ProcessFile.Query>(), default), Times.Once);
    }

    [Fact]
    public async Task GetTransactionSummariesEndpoint_ShouldReturnTransactionSummaries()
    {
        // Arrange
        var transactionSummaries = new SmartFaker<Kaesseli.Contracts.Integration.TransactionSummary>().Generate(
            count: 3
        );
        _getTransactionSummariesMock
            .Setup(m => m.Handle(default))
            .ReturnsAsync(transactionSummaries);

        // Act
        var response = await _client.GetAsync(requestUri: "/transactionSummary");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        _getTransactionSummariesMock.Verify(
            m => m.Handle(default),
            Times.Once
        );
    }

    [Fact]
    public async Task GetNextOpenTransactionEndpoint_ShouldReturnNextOpenTransaction()
    {
        // Arrange
        var nextOpenTransaction = new SmartFaker<OpenTransaction>().Generate();
        _getNextOpenTransactionMock
            .Setup(m => m.Handle(It.IsAny<GetNextOpenTransaction.Query>(), default))
            .ReturnsAsync(nextOpenTransaction);

        // Act
        var response = await _client.GetAsync(requestUri: "/transaction/nextOpen");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        _getNextOpenTransactionMock.Verify(
            m => m.Handle(It.IsAny<GetNextOpenTransaction.Query>(), default),
            Times.Once
        );
    }
}
