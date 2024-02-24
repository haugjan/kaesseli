using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Kaesseli.Application.Integration;
using Kaesseli.Server.Integration;
using Kaesseli.TestUtilities.Faker;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
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
                         })
                     .Configure(
                         app =>
                         {
                             app.UseRouting();
                             app.UseEndpoints(
                                 endpoints =>
                                 {
                                     endpoints.MapIntegrationEndpoints();
                                 });
                         }));

        _client = server.CreateClient();
    }

    [Fact]
    public async Task AddJournalEntryEndpoint_ShouldReturnCreatedResult()
    {
        // Arrange
        var guid = Guid.NewGuid();
        _mediatorMock.Setup(m => m.Send(It.IsAny<ProcessCamtFileCommand>(), default)).ReturnsAsync(guid);

        var addJournalEntryCommand = new SmartFaker<ProcessCamtFileCommand>().Generate();
        var content = new StringContent(
            content: JsonSerializer.Serialize(addJournalEntryCommand),
            Encoding.UTF8,
            mediaType: "application/json");

        // Act
        var response = await _client.PostAsync(requestUri: "/camt", content);

        // Assert
        response.StatusCode.Should()
                .Be(
                    HttpStatusCode.Created,
                    because: $"there should be no failure, but server responded '{await response.Content.ReadAsStringAsync()}'");
        response.Headers.Location.Should()
                .BeEquivalentTo(expectation: new Uri(uriString: $"/camt/{guid}", UriKind.Relative));
        _mediatorMock.Verify(m => m.Send(It.IsAny<ProcessCamtFileCommand>(), default), Times.Once);
    }
}