using System.Net;
using System.Net.Http.Headers;
using FluentAssertions;
using Kaesseli.Application.Integration.Camt;
using Kaesseli.Application.Integration.NextOpenTransaction;
using Kaesseli.Application.Integration.TransactionQuery;
using Kaesseli.Server.Integration;
using Kaesseli.TestUtilities.Faker;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Kaesseli.Server.Test.Integration;

public class IntegrationApiExtensionsTests
{
    private readonly HttpClient _client;
    private readonly Mock<IMediator> _mediatorMock;

    public IntegrationApiExtensionsTests()
    {
        _mediatorMock = new Mock<IMediator>();

        var server = new TestServer(
            builder: new WebHostBuilder()
                     .ConfigureServices(
                         services =>
                         {
                             services.AddRouting();
                             services.AddSingleton(_mediatorMock.Object);
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
        _mediatorMock.Setup(m => m.Send(It.IsAny<ProcessCamtFileCommand>(), default)).ReturnsAsync(guid);

        var formContent = new MultipartFormDataContent();
        var accountId = Guid.NewGuid();
        var fileContent = new ByteArrayContent(content: "Dummy File Content"u8.ToArray());
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(input: "multipart/form-data");
        formContent.Add(fileContent, name: "file", fileName: "dummy_file.txt");

        var accountIdContent = new StringContent(content: accountId.ToString());
        formContent.Add(accountIdContent, name: "accountId");
        // Act
        var response = await _client.PostAsync(requestUri: "/camt/upload", formContent);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        _mediatorMock.Verify(m => m.Send(It.IsAny<ProcessCamtFileCommand>(), default), Times.Once);
    }

    [Fact]
    public async Task GetTransactionSummariesEndpoint_ShouldReturnTransactionSummaries()
    {
        // Arrange
        var transactionSummaries = new SmartFaker<GetTransactionSummariesQueryResult>().Generate(count: 3);
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetTransactionSummariesQuery>(), default)).ReturnsAsync(transactionSummaries);

        // Act
        var response = await _client.GetAsync(requestUri: "/transactionSummary");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        _mediatorMock.Verify(m => m.Send(It.IsAny<GetTransactionSummariesQuery>(), default), Times.Once);
    }


    [Fact]
    public async Task G2etTransactionSummariesEndpoint_ShouldReturnTransactionSummaries()
    {
        // Arrange
        var nextOpenTransaction = new SmartFaker<GetNextOpenTransactionQueryResult>().Generate();
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetNextOpenTransactionQuery>(), default)).ReturnsAsync(nextOpenTransaction);

        // Act
        var response = await _client.GetAsync(requestUri: "/transaction/nextOpen");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        _mediatorMock.Verify(m => m.Send(It.IsAny<GetNextOpenTransactionQuery>(), default), Times.Once);
    }
}