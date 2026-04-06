using System.Net;
using System.Text;
using System.Text.Json;
using Kaesseli.Features.Budget;
using Kaesseli.Test.Faker;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Kaesseli.Test.Features.Budget;

public class BudgetApiTests : IAsyncLifetime
{
    private HttpClient _client = null!;
    private readonly SetBudget.IHandler _setBudgetMock = Substitute.For<SetBudget.IHandler>();
    private readonly GetBudgetEntries.IHandler _getBudgetEntriesMock = Substitute.For<GetBudgetEntries.IHandler>();

    public async Task InitializeAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddRouting();
        builder.Services.AddSingleton(_setBudgetMock);
        builder.Services.AddSingleton(_getBudgetEntriesMock);

        var app = builder.Build();
        app.MapBudgetEndpoints();

        await app.StartAsync();
        _client = app.GetTestClient();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task SetBudgetCommandEndpoint_ShouldReturnCreatedResult()
    {
        // Arrange
        var guid = Guid.NewGuid();
        _setBudgetMock
            .Handle(Arg.Any<SetBudget.Query>(), Arg.Any<CancellationToken>())
            .Returns(guid);

        var setBudgetCommand = new SmartFaker<SetBudget.Query>().Generate();
        var content = new StringContent(
            content: JsonSerializer.Serialize(setBudgetCommand),
            Encoding.UTF8,
            mediaType: "application/json"
        );

        // Act
        var response = await _client.PostAsync(requestUri: "/budgetEntry", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        response.Headers.Location.ShouldBe(
            new Uri(uriString: $"/budgetEntry/{guid}", UriKind.Relative)
        );
        await _setBudgetMock.Received(1).Handle(Arg.Any<SetBudget.Query>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetBudgetEntriesEndpoint_ShouldReturnBudgetEntries()
    {
        // Arrange
        var budgetEntries = new SmartFaker<Kaesseli.Contracts.Budget.BudgetEntry>().Generate(count: 3);
        _getBudgetEntriesMock
            .Handle(Arg.Any<GetBudgetEntries.Query>(), Arg.Any<CancellationToken>())
            .Returns(budgetEntries);

        var accountId = Guid.NewGuid();
        var periodId = Guid.NewGuid();
        var from = new DateOnly(year: 2023, month: 1, day: 1);
        var to = new DateOnly(year: 2023, month: 12, day: 31);
        var queryString =
            $"?accountingPeriodId={periodId}&accountId={accountId}&from={from:yyyy-MM-dd}&to={to:yyyy-MM-dd}";

        // Act
        var response = await _client.GetAsync(requestUri: $"/budgetEntry{queryString}");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        await _getBudgetEntriesMock.Received(1)
            .Handle(Arg.Any<GetBudgetEntries.Query>(), Arg.Any<CancellationToken>());
    }
}
