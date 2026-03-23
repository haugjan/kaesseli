using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Kaesseli.Application.Journal;
using Kaesseli.Domain.Accounts;
using Kaesseli.TestUtilities.Faker;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Kaesseli.Server.Test.Journal;

public class JournalApiExtensionsTests
{
    private readonly HttpClient _client;
    private readonly Mock<AddJournalEntry.IHandler> _addJournalEntryMock = new();
    private readonly Mock<GetJournalEntries.IHandler> _getJournalEntriesMock = new();

    public JournalApiExtensionsTests()
    {
        var server = new TestServer(
            builder: new WebHostBuilder()
                     .ConfigureServices(
                         services =>
                         {
                             services.AddRouting();
                             services.AddSingleton(_addJournalEntryMock.Object);
                             services.AddSingleton(_getJournalEntriesMock.Object);
                         })
                     .Configure(
                         app =>
                         {
                             app.UseRouting();
                             app.UseEndpoints(
                                 endpoints =>
                                 {
                                     endpoints.MapJournalEndpoints();
                                 });
                         }));

        _client = server.CreateClient();
    }

    [Fact]
    public async Task AddJournalEntryEndpoint_ShouldReturnCreatedResult()
    {
        // Arrange
        var guid = Guid.NewGuid();
        _addJournalEntryMock.Setup(m => m.Handle(It.IsAny<AddJournalEntry.Query>(), default)).ReturnsAsync(guid);

        var addJournalEntryCommand = new SmartFaker<AddJournalEntry.Query>().Generate();
        var content = new StringContent(
            content: JsonSerializer.Serialize(addJournalEntryCommand),
            Encoding.UTF8,
            mediaType: "application/json");

        // Act
        var response = await _client.PostAsync(requestUri: "/journalEntry", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should()
                .BeEquivalentTo(expectation: new Uri(uriString: $"/journalEntry/{guid}", UriKind.Relative));
        _addJournalEntryMock.Verify(m => m.Handle(It.IsAny<AddJournalEntry.Query>(), default), Times.Once);
    }


    [Fact]
    public async Task GetJournalEntriesEndpoint_ShouldReturnJournalEntries()
    {
        // Arrange
        var journalEntries = new SmartFaker<GetJournalEntries.Result>().Generate(count: 3);
        _getJournalEntriesMock.Setup(m => m.Handle(It.IsAny<GetJournalEntries.Query>(), default)).ReturnsAsync(journalEntries);

        var accountId = Guid.NewGuid();
        var periodId = Guid.NewGuid();
        const AccountType accountType = AccountType.Liability;
        var queryString =
            $"?accountingPeriodId={periodId}&accountId={accountId}&accountType={accountType}";

        // Act
        var response = await _client.GetAsync(requestUri: $"/journalEntry{queryString}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        _getJournalEntriesMock
            .Verify(m => m.Handle(
                                 It.Is<GetJournalEntries.Query>(query
                                                                   => query.AccountingPeriodId == periodId
                                                                             && query.AccountType == accountType
                                                                   && query.AccountId == accountId), default), Times.Once);
    }
}
