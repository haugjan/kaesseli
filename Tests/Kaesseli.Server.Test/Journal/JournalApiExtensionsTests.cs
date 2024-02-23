using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Kaesseli.Application.Journal;
using Kaesseli.Server.Journal;
using Kaesseli.TestUtilities.Faker;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Kaesseli.Server.Test.Journal;

public class JournalApiExtensionsTests
{
    private readonly HttpClient _client;
    private readonly Mock<IMediator> _mediatorMock;

    public JournalApiExtensionsTests()
    {
        _mediatorMock = new Mock<IMediator>();

        var server = new TestServer(
            builder: new WebHostBuilder()
                     .ConfigureServices(
                         services =>
                         {
                             services.AddRouting();
                             services.AddSingleton(_mediatorMock.Object);
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
        _mediatorMock.Setup(m => m.Send(It.IsAny<AddJournalEntryCommand>(), default)).ReturnsAsync(guid);

        var addJournalEntryCommand = new SmartFaker<AddJournalEntryCommand>().Generate();
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
        _mediatorMock.Verify(m => m.Send(It.IsAny<AddJournalEntryCommand>(), default), Times.Once);
    }

   
    [Fact]
    public async Task GetJournalEntriesEndpoint_ShouldReturnJournalEntries()
    {
        // Arrange
        var journalEntries = new SmartFaker<GetJournalEntriesQueryResult>().Generate(count: 3);
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetJournalEntriesQuery>(), default)).ReturnsAsync(journalEntries);

        var debitAccountId = Guid.NewGuid();
        var creditAccountId = Guid.NewGuid();
        var from = new DateOnly(year: 2023, month: 1, day: 1);
        var to = new DateOnly(year: 2023, month: 12, day: 31);
        var queryString =
            $"?debitAccountId={debitAccountId}&creditAccountId={creditAccountId}&from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}";

        // Act
        var response = await _client.GetAsync(requestUri: $"/journalEntry{queryString}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        _mediatorMock.Verify(m => m.Send(It.IsAny<GetJournalEntriesQuery>(), default), Times.Once);
    }
}