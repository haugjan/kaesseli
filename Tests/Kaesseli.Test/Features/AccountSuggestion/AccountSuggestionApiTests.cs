using System.Net;
using System.Net.Http.Json;
using Kaesseli.Features.AccountSuggestion;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;
using Xunit;
using JobStatusContract = Kaesseli.Contracts.AccountSuggestion.AccountSuggestionJobStatus;

namespace Kaesseli.Test.Features.AccountSuggestions;

public class AccountSuggestionApiTests : IAsyncLifetime
{
    private HttpClient _client = null!;
    private readonly GenerateAccountSuggestions.IRunner _runnerMock =
        Substitute.For<GenerateAccountSuggestions.IRunner>();

    public async Task InitializeAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddRouting();
        builder.Services.AddSingleton(_runnerMock);

        var app = builder.Build();
        app.MapAccountSuggestionEndpoints();
        await app.StartAsync();
        _client = app.GetTestClient();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Generate_StartsJob_Returns202_WhenAccepted()
    {
        var status = new AccountSuggestionJobStatus();
        status.TryStart(Guid.NewGuid(), total: 0, DateTimeOffset.UtcNow);
        _runnerMock.Status.Returns(status);
        _runnerMock.TryStartInBackground().Returns(true);

        var response = await _client.PostAsync("/accountSuggestion/generate", content: null);

        response.StatusCode.ShouldBe(HttpStatusCode.Accepted);
        var body = await response.Content.ReadFromJsonAsync<JobStatusContract>();
        body.ShouldNotBeNull();
        body!.Started.ShouldBe(true);
    }

    [Fact]
    public async Task Generate_ReturnsOk_WhenAlreadyRunning()
    {
        var status = new AccountSuggestionJobStatus();
        status.TryStart(Guid.NewGuid(), total: 0, DateTimeOffset.UtcNow);
        _runnerMock.Status.Returns(status);
        _runnerMock.TryStartInBackground().Returns(false);

        var response = await _client.PostAsync("/accountSuggestion/generate", content: null);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JobStatusContract>();
        body.ShouldNotBeNull();
        body!.Started.ShouldBe(false);
        body.IsRunning.ShouldBeTrue();
    }

    [Fact]
    public async Task Status_ReturnsCurrentStatus()
    {
        var status = new AccountSuggestionJobStatus();
        status.TryStart(Guid.NewGuid(), total: 10, DateTimeOffset.UtcNow);
        status.IncrementProcessed();
        status.IncrementProcessed();
        _runnerMock.Status.Returns(status);

        var response = await _client.GetAsync("/accountSuggestion/status");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JobStatusContract>();
        body.ShouldNotBeNull();
        body!.IsRunning.ShouldBeTrue();
        body.Processed.ShouldBe(2);
        body.Total.ShouldBe(10);
        body.Started.ShouldBeNull();
    }
}
