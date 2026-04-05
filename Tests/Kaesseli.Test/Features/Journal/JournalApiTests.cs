using System.Net;
using System.Text;
using System.Text.Json;
using Kaesseli.Features.Journal;
using Kaesseli.Features.Accounts;
using Kaesseli.Test.Faker;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shouldly;
using Xunit;

namespace Kaesseli.Test.Features.Journal;

public class JournalApiTests : IAsyncLifetime
{
    private HttpClient _client = null!;
    private readonly Mock<AddJournalEntry.IHandler> _addJournalEntryMock = new();
    private readonly Mock<GetJournalEntries.IHandler> _getJournalEntriesMock = new();

    public async Task InitializeAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddRouting();
        builder.Services.AddSingleton(_addJournalEntryMock.Object);
        builder.Services.AddSingleton(_getJournalEntriesMock.Object);

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
            .Setup(m => m.Handle(It.IsAny<AddJournalEntry.Query>(), default))
            .ReturnsAsync(guid);

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
        _addJournalEntryMock.Verify(
            m => m.Handle(It.IsAny<AddJournalEntry.Query>(), default),
            Times.Once
        );
    }

    [Fact]
    public async Task GetJournalEntriesEndpoint_ShouldReturnJournalEntries()
    {
        // Arrange
        var journalEntries = new SmartFaker<Kaesseli.Contracts.Journal.JournalEntry>().Generate(count: 3);
        _getJournalEntriesMock
            .Setup(m => m.Handle(It.IsAny<GetJournalEntries.Query>(), default))
            .ReturnsAsync(journalEntries);

        var accountId = Guid.NewGuid();
        var periodId = Guid.NewGuid();
        const AccountType accountType = AccountType.Liability;
        var queryString =
            $"?accountingPeriodId={periodId}&accountId={accountId}&accountType={accountType}";

        // Act
        var response = await _client.GetAsync(requestUri: $"/journalEntry{queryString}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        _getJournalEntriesMock.Verify(
            m =>
                m.Handle(
                    It.Is<GetJournalEntries.Query>(query =>
                        query.AccountingPeriodId == periodId
                        && query.AccountType == accountType
                        && query.AccountId == accountId
                    ),
                    default
                ),
            Times.Once
        );
    }
}
