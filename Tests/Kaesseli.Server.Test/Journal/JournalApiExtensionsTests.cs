using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Kaesseli.Application.Journal;
using Kaesseli.Domain.Accounts;
using Kaesseli.TestUtilities.Faker;
using MediatR;
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

        var accountId = Guid.NewGuid();
        var periodId = Guid.NewGuid();
        const AccountType accountType = AccountType.Liability;
        var queryString =
            $"?accountingPeriodId={periodId}&accountId={accountId}&accountType={accountType}";

        // Act
        var response = await _client.GetAsync(requestUri: $"/journalEntry{queryString}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        _mediatorMock
            .Verify(m => m.Send(
                                 It.Is<GetJournalEntriesQuery>(query 
                                                                   => query.AccountingPeriodId == periodId 
                                                                             && query.AccountType == accountType 
                                                                   && query.AccountId == accountId), default), Times.Once);
    }
}