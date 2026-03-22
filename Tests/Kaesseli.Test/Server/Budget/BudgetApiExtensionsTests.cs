using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Kaesseli.Application.Budget;
using Kaesseli.TestUtilities.Faker;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Kaesseli.Server.Test.Budget;

public class BudgetApiExtensionsTests
{
    private readonly HttpClient _client;
    private readonly Mock<ISetBudgetCommandHandler> _setBudgetMock = new();
    private readonly Mock<IGetBudgetEntriesQueryHandler> _getBudgetEntriesMock = new();

    public BudgetApiExtensionsTests()
    {
        var server = new TestServer(
            builder: new WebHostBuilder()
                     .ConfigureServices(
                         services =>
                         {
                             services.AddRouting();
                             services.AddSingleton(_setBudgetMock.Object);
                             services.AddSingleton(_getBudgetEntriesMock.Object);
                         })
                     .Configure(
                         app =>
                         {
                             app.UseRouting();
                             app.UseEndpoints(
                                 endpoints =>
                                 {
                                     endpoints.MapBudgetEndpoints();
                                 });
                         }));

        _client = server.CreateClient();
    }

    [Fact]
    public async Task SetBudgetCommandEndpoint_ShouldReturnCreatedResult()
    {
        // Arrange
        var guid = Guid.NewGuid();
        _setBudgetMock.Setup(m => m.Handle(It.IsAny<SetBudgetCommand>(), default)).ReturnsAsync(guid);

        var setBudgetCommand = new SmartFaker<SetBudgetCommand>().Generate();
        var content = new StringContent(
            content: JsonSerializer.Serialize(setBudgetCommand),
            Encoding.UTF8,
            mediaType: "application/json");

        // Act
        var response = await _client.PostAsync(requestUri: "/budgetEntry", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should()
                .BeEquivalentTo(expectation: new Uri(uriString: $"/budgetEntry/{guid}", UriKind.Relative));
        _setBudgetMock.Verify(m => m.Handle(It.IsAny<SetBudgetCommand>(), default), Times.Once);
    }

    [Fact]
    public async Task GetBudgetEntriesEndpoint_ShouldReturnBudgetEntries()
    {
        // Arrange
        var budgetEntries = new SmartFaker<GetBudgetEntriesQueryResult>().Generate(count: 3);
        _getBudgetEntriesMock.Setup(m => m.Handle(It.IsAny<GetBudgetEntriesQuery>(), default)).ReturnsAsync(budgetEntries);

        var accountId = Guid.NewGuid();
        var periodId = Guid.NewGuid();
        var from = new DateOnly(year: 2023, month: 1, day: 1);
        var to = new DateOnly(year: 2023, month: 12, day: 31);
        var queryString = $"?accountingPeriodId={periodId}&accountId={accountId}&from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}";

        // Act
        var response = await _client.GetAsync(requestUri: $"/budgetEntry{queryString}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        _getBudgetEntriesMock.Verify(m => m.Handle(It.IsAny<GetBudgetEntriesQuery>(), default), Times.Once);
    }
}
