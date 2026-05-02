using System.Net;
using System.Text;
using System.Text.Json;
using Kaesseli.Features.Accounts;
using Kaesseli.Features.Journal;
using Kaesseli.Test.Faker;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Kaesseli.Test.Features.Journal;

public class JournalApiTests : IAsyncLifetime
{
    private HttpClient _client = null!;
    private readonly AddJournalEntry.IHandler _addJournalEntryMock =
        Substitute.For<AddJournalEntry.IHandler>();
    private readonly AddOpeningBalance.IHandler _addOpeningBalanceMock =
        Substitute.For<AddOpeningBalance.IHandler>();
    private readonly DeleteJournalEntry.IHandler _deleteJournalEntryMock =
        Substitute.For<DeleteJournalEntry.IHandler>();
    private readonly GetJournalEntries.IHandler _getJournalEntriesMock =
        Substitute.For<GetJournalEntries.IHandler>();

    public async Task InitializeAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddRouting();
        builder.Services.AddSingleton(_addJournalEntryMock);
        builder.Services.AddSingleton(_addOpeningBalanceMock);
        builder.Services.AddSingleton(_deleteJournalEntryMock);
        builder.Services.AddSingleton(_getJournalEntriesMock);

        var app = builder.Build();
        app.MapJournalEndpoints();

        await app.StartAsync();
        _client = app.GetTestClient();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task AddJournalEntryEndpoint_ShouldReturnCreatedResult()
    {
        // Arrange
        var guid = Guid.NewGuid();
        _addJournalEntryMock
            .Handle(Arg.Any<AddJournalEntry.Query>(), Arg.Any<CancellationToken>())
            .Returns(guid);

        var addJournalEntryCommand = new SmartFaker<AddJournalEntry.Query>().Generate();
        var content = new StringContent(
            content: JsonSerializer.Serialize(addJournalEntryCommand),
            Encoding.UTF8,
            mediaType: "application/json"
        );

        // Act
        var response = await _client.PostAsync(requestUri: "/journalEntry", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        response.Headers.Location.ShouldBe(
            new Uri(uriString: $"/journalEntry/{guid}", UriKind.Relative)
        );
        await _addJournalEntryMock
            .Received(1)
            .Handle(Arg.Any<AddJournalEntry.Query>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetJournalEntriesEndpoint_ShouldReturnJournalEntries()
    {
        // Arrange
        var journalEntries = new SmartFaker<Kaesseli.Contracts.Journal.JournalEntry>().Generate(
            count: 3
        );
        _getJournalEntriesMock
            .Handle(Arg.Any<GetJournalEntries.Query>(), Arg.Any<CancellationToken>())
            .Returns(journalEntries);

        var accountId = Guid.NewGuid();
        var periodId = Guid.NewGuid();
        const AccountType accountType = AccountType.Liability;
        var queryString =
            $"?accountingPeriodId={periodId}&accountId={accountId}&accountType={accountType}";

        // Act
        var response = await _client.GetAsync(requestUri: $"/journalEntry{queryString}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        await _getJournalEntriesMock
            .Received(1)
            .Handle(
                Arg.Is<GetJournalEntries.Query>(query =>
                    query.AccountingPeriodId == periodId
                    && query.AccountType == accountType
                    && query.AccountId == accountId
                ),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task DeleteJournalEntryEndpoint_ShouldReturnNoContent()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync(requestUri: $"/journalEntry/{id}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        await _deleteJournalEntryMock
            .Received(1)
            .Handle(
                Arg.Is<DeleteJournalEntry.Query>(query => query.Id == id),
                Arg.Any<CancellationToken>()
            );
    }
}
