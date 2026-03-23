using System.Net;
using System.Net.Http.Headers;
using FluentAssertions;
using Kaesseli.Application.Integration.FileImport;
using Kaesseli.Application.Integration.NextOpenTransaction;
using Kaesseli.Application.Integration.TransactionQuery;
using Kaesseli.TestUtilities.Faker;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Kaesseli.Server.Test.Integration;

public class IntegrationApiExtensionsTests
{
    private readonly HttpClient _client;
    private readonly Mock<ProcessFile.IHandler> _processFileMock = new();
    private readonly Mock<GetTransactionSummaries.IHandler> _getTransactionSummariesMock = new();
    private readonly Mock<GetTransactions.IHandler> _getTransactionsMock = new();
    private readonly Mock<GetNextOpenTransaction.IHandler> _getNextOpenTransactionMock = new();
    private readonly Mock<GetTotalOpenTransaction.IHandler> _getTotalOpenTransactionMock = new();
    private readonly Mock<AssignOpenTransaction.IHandler> _assignOpenTransactionMock = new();
    private readonly Mock<SplitOpenTransaction.IHandler> _splitOpenTransactionMock = new();

    public IntegrationApiExtensionsTests()
    {
        var server = new TestServer(
            builder: new WebHostBuilder()
                     .ConfigureServices(
                         services =>
                         {
                             services.AddRouting();
                             services.AddSingleton(_processFileMock.Object);
                             services.AddSingleton(_getTransactionSummariesMock.Object);
                             services.AddSingleton(_getTransactionsMock.Object);
                             services.AddSingleton(_getNextOpenTransactionMock.Object);
                             services.AddSingleton(_getTotalOpenTransactionMock.Object);
                             services.AddSingleton(_assignOpenTransactionMock.Object);
                             services.AddSingleton(_splitOpenTransactionMock.Object);
                             services.AddAntiforgery();
                             services.AddLogging(
                                 loggingBuilder =>
                                 {
                                     loggingBuilder.AddConsole();
                                     loggingBuilder.AddDebug();
                                 });
                         })
                     .Configure(
                         app =>
                         {
                             app.UseRouting();
                             app.UseAntiforgery();
                             app.UseEndpoints(endpoints => endpoints.MapIntegrationEndpoints());
                         }));

        _client = server.CreateClient();
    }

    [Fact]
    public async Task CamtUploadEndpoint_ShouldReturnCreatedResult()
    {
        // Arrange
        var guid = Guid.NewGuid();
        _processFileMock.Setup(m => m.Handle(It.IsAny<ProcessFile.Query>(), default)).ReturnsAsync(guid);

        var formContent = new MultipartFormDataContent();
        var accountId = Guid.NewGuid();
        var fileContent = new ByteArrayContent(content: "Dummy File Content"u8.ToArray());
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(input: "multipart/form-data");
        formContent.Add(fileContent, name: "file", fileName: "dummy_file.txt");

        var accountIdContent = new StringContent(content: accountId.ToString());
        formContent.Add(accountIdContent, name: "accountId");
        // Act
        var response = await _client.PostAsync(requestUri: "/file/upload", formContent);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        _processFileMock.Verify(m => m.Handle(It.IsAny<ProcessFile.Query>(), default), Times.Once);
    }

    [Fact]
    public async Task GetTransactionSummariesEndpoint_ShouldReturnTransactionSummaries()
    {
        // Arrange
        var transactionSummaries = new SmartFaker<GetTransactionSummaries.Result>().Generate(count: 3);
        _getTransactionSummariesMock.Setup(m => m.Handle(It.IsAny<GetTransactionSummaries.Query>(), default)).ReturnsAsync(transactionSummaries);

        // Act
        var response = await _client.GetAsync(requestUri: "/transactionSummary");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        _getTransactionSummariesMock.Verify(m => m.Handle(It.IsAny<GetTransactionSummaries.Query>(), default), Times.Once);
    }


    [Fact]
    public async Task G2etTransactionSummariesEndpoint_ShouldReturnTransactionSummaries()
    {
        // Arrange
        var nextOpenTransaction = new SmartFaker<GetNextOpenTransaction.Result>().Generate();
        _getNextOpenTransactionMock.Setup(m => m.Handle(It.IsAny<GetNextOpenTransaction.Query>(), default)).ReturnsAsync(nextOpenTransaction);

        // Act
        var response = await _client.GetAsync(requestUri: "/transaction/nextOpen");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        _getNextOpenTransactionMock.Verify(m => m.Handle(It.IsAny<GetNextOpenTransaction.Query>(), default), Times.Once);
    }
}
