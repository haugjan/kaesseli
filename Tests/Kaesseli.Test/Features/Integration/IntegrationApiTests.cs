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
using NSubstitute;
using Shouldly;
using Xunit;

namespace Kaesseli.Test.Features.Integration;

public class IntegrationApiTests : IAsyncLifetime
{
    private HttpClient _client = null!;
    private readonly ProcessFile.IHandler _processFileMock = Substitute.For<ProcessFile.IHandler>();
    private readonly GetTransactionSummaries.IHandler _getTransactionSummariesMock = Substitute.For<GetTransactionSummaries.IHandler>();
    private readonly GetTransactions.IHandler _getTransactionsMock = Substitute.For<GetTransactions.IHandler>();
    private readonly GetNextOpenTransaction.IHandler _getNextOpenTransactionMock = Substitute.For<GetNextOpenTransaction.IHandler>();
    private readonly GetTotalOpenTransaction.IHandler _getTotalOpenTransactionMock = Substitute.For<GetTotalOpenTransaction.IHandler>();
    private readonly AssignOpenTransaction.IHandler _assignOpenTransactionMock = Substitute.For<AssignOpenTransaction.IHandler>();
    private readonly SplitOpenTransaction.IHandler _splitOpenTransactionMock = Substitute.For<SplitOpenTransaction.IHandler>();

    public async Task InitializeAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddRouting();
        builder.Services.AddSingleton(_processFileMock);
        builder.Services.AddSingleton(_getTransactionSummariesMock);
        builder.Services.AddSingleton(_getTransactionsMock);
        builder.Services.AddSingleton(_getNextOpenTransactionMock);
        builder.Services.AddSingleton(_getTotalOpenTransactionMock);
        builder.Services.AddSingleton(_assignOpenTransactionMock);
        builder.Services.AddSingleton(_splitOpenTransactionMock);
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
            .Handle(Arg.Any<ProcessFile.Query>(), Arg.Any<CancellationToken>())
            .Returns(guid);

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
        await _processFileMock.Received(1).Handle(Arg.Any<ProcessFile.Query>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetTransactionSummariesEndpoint_ShouldReturnTransactionSummaries()
    {
        // Arrange
        var transactionSummaries = new SmartFaker<Kaesseli.Contracts.Integration.TransactionSummary>().Generate(
            count: 3
        );
        _getTransactionSummariesMock
            .Handle(Arg.Any<CancellationToken>())
            .Returns(transactionSummaries);

        // Act
        var response = await _client.GetAsync(requestUri: "/transactionSummary");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        await _getTransactionSummariesMock.Received(1)
            .Handle(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetNextOpenTransactionEndpoint_ShouldReturnNextOpenTransaction()
    {
        // Arrange
        var nextOpenTransaction = new SmartFaker<OpenTransaction>().Generate();
        _getNextOpenTransactionMock
            .Handle(Arg.Any<GetNextOpenTransaction.Query>(), Arg.Any<CancellationToken>())
            .Returns(nextOpenTransaction);

        // Act
        var response = await _client.GetAsync(requestUri: "/transaction/nextOpen");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        await _getNextOpenTransactionMock.Received(1)
            .Handle(Arg.Any<GetNextOpenTransaction.Query>(), Arg.Any<CancellationToken>());
    }
}
