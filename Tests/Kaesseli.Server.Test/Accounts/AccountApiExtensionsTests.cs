using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Kaesseli.Application.Accounts;
using Kaesseli.Server.Accounts;
using Kaesseli.TestUtilities.Faker;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Kaesseli.Server.Test.Accounts;

public class AccountApiExtensionsTests
{
    private readonly HttpClient _client;
    private readonly Mock<IMediator> _mediatorMock;

    public AccountApiExtensionsTests()
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
                                     endpoints.MapAccountEndpoints();
                                 });
                         }));

        _client = server.CreateClient();
    }

    [Fact]
    public async Task GetAccountEndpoint_ShouldReturnAccounts()
    {
        // Arrange
        var accounts = new SmartFaker<GetAccountsQueryResult>().Generate(count: 3);
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetAccountsQuery>(), default)).ReturnsAsync(accounts);

        // Act
        var response = await _client.GetAsync(requestUri: "/account");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        _mediatorMock.Verify(m => m.Send(It.IsAny<GetAccountsQuery>(), default), Times.Once);
    }

    [Fact]
    public async Task AddAccountEndpoint_ShouldReturnCreatedResult()
    {
        // Arrange
        var guid = Guid.NewGuid();
        _mediatorMock.Setup(m => m.Send(It.IsAny<AddAccountCommand>(), default)).ReturnsAsync(guid);

        var addAccountCommand = new SmartFaker<AddAccountCommand>().Generate();
        var content = new StringContent(
            content: JsonSerializer.Serialize(value: addAccountCommand),
            Encoding.UTF8,
            mediaType: "application/json");

        // Act
        var response = await _client.PostAsync(requestUri: "/account", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        if (_client.BaseAddress != null)
            response.Headers.Location.Should()
                    .BeEquivalentTo(expectation: new Uri( uriString: $"/account/{guid}", UriKind.Relative));
        _mediatorMock.Verify(m => m.Send(It.IsAny<AddAccountCommand>(), default), Times.Once);
    }
}