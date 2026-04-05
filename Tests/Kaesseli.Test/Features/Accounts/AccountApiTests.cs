using System.Net;
using System.Text;
using System.Text.Json;
using Kaesseli.Features.Accounts;
using Kaesseli.Test.Faker;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shouldly;
using Xunit;

namespace Kaesseli.Test.Features.Accounts;

public class AccountApiTests : IAsyncLifetime
{
    private HttpClient _client = null!;
    private readonly Mock<GetAccounts.IHandler> _getAccountsMock = new();
    private readonly Mock<GetAccountingPeriods.IHandler> _getAccountingPeriodsMock = new();
    private readonly Mock<GetAccount.IHandler> _getAccountMock = new();
    private readonly Mock<GetAccountsSummary.IHandler> _getAccountsSummaryMock = new();
    private readonly Mock<GetFinancialOverview.IHandler> _getFinancialOverviewMock = new();
    private readonly Mock<AddAccount.IHandler> _addAccountMock = new();
    private readonly Mock<AddAccountingPeriod.IHandler> _addAccountingPeriodMock = new();

    public async Task InitializeAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddRouting();
        builder.Services.AddSingleton(_getAccountsMock.Object);
        builder.Services.AddSingleton(_getAccountingPeriodsMock.Object);
        builder.Services.AddSingleton(_getAccountMock.Object);
        builder.Services.AddSingleton(_getAccountsSummaryMock.Object);
        builder.Services.AddSingleton(_getFinancialOverviewMock.Object);
        builder.Services.AddSingleton(_addAccountMock.Object);
        builder.Services.AddSingleton(_addAccountingPeriodMock.Object);

        var app = builder.Build();
        app.MapAccountEndpoints();

        await app.StartAsync();
        _client = app.GetTestClient();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetAccountsEndpoint_ShouldReturnAccounts()
    {
        // Arrange
        var periodId = Guid.NewGuid();
        var accounts = new SmartFaker<GetAccountsSummaryContract.Result>().Generate(count: 3);
        _getAccountsSummaryMock
            .Setup(m => m.Handle(It.IsAny<GetAccountsSummary.Query>(), default))
            .ReturnsAsync(accounts);

        // Act
        var response = await _client.GetAsync(
            requestUri: $"/accountingPeriod/{periodId}/accountSummary"
        );

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        _getAccountsSummaryMock.Verify(
            m => m.Handle(It.IsAny<GetAccountsSummary.Query>(), default),
            Times.Once
        );
    }

    [Fact]
    public async Task GetAccountEndpoint_ShouldReturnSingleAccount()
    {
        // Arrange
        var periodId = Guid.NewGuid();
        var accounts = new SmartFaker<GetAccountsContract.Result>().Generate(count: 3);
        var expectedAccount = accounts[index: 1];
        _getAccountMock
            .Setup(m =>
                m.Handle(It.Is<GetAccount.Query>(x => x.AccountId == expectedAccount.Id), default)
            )
            .ReturnsAsync(
                (GetAccount.Query _, CancellationToken _) =>
                    new GetAccountContract.Result(
                        Id: expectedAccount.Id,
                        Name: expectedAccount.Name,
                        Icon: expectedAccount.Icon,
                        IconColor: expectedAccount.IconColor,
                        Type: expectedAccount.TypeId.DisplayName(),
                        TypeId: expectedAccount.TypeId,
                        AccountBalance: 10,
                        Budget: 11,
                        BudgetPerMonth: null,
                        BudgetPerYear: null,
                        CurrentBudget: 13,
                        BudgetBalance: 12,
                        Entries: Array.Empty<GetAccountContract.ResultEntry>())
            );

        // Act
        var response = await _client.GetAsync(
            requestUri: $"/accountingPeriod/{periodId}/account/{expectedAccount.Id}"
        );
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var accountResponse = JsonSerializer.Deserialize<GetAccountContract.Result>(
            json: await response.Content.ReadAsStringAsync(),
            options
        );
        Assert.Equivalent(expectedAccount, accountResponse);
        _getAccountMock.Verify(
            m =>
                m.Handle(
                    It.Is<GetAccount.Query>(query =>
                        query.AccountId == expectedAccount.Id
                        && query.AccountingPeriodId == periodId
                    ),
                    default
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task AddAccountEndpoint_ShouldReturnCreatedResult()
    {
        // Arrange
        var guid = Guid.NewGuid();
        _addAccountMock
            .Setup(m => m.Handle(It.IsAny<AddAccount.Query>(), default))
            .ReturnsAsync(guid);

        var addAccountCommand = new SmartFaker<AddAccount.Query>().Generate();
        var content = new StringContent(
            content: JsonSerializer.Serialize(addAccountCommand),
            Encoding.UTF8,
            mediaType: "application/json"
        );

        // Act
        var response = await _client.PostAsync(requestUri: "/account", content);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        if (_client.BaseAddress != null)
        {
            response.Headers.Location.ShouldBe(
                new Uri(uriString: $"/account/{guid}", UriKind.Relative)
            );
        }

        _addAccountMock.Verify(m => m.Handle(It.IsAny<AddAccount.Query>(), default), Times.Once);
    }

    [Fact]
    public async Task AddAccountingPeriodEndpoint_ShouldReturnCreatedResult()
    {
        // Arrange
        var expectedGuid = Guid.NewGuid();
        _addAccountingPeriodMock
            .Setup(m => m.Handle(It.IsAny<AddAccountingPeriod.Query>(), default))
            .ReturnsAsync(expectedGuid);

        var addAccountingPeriodCommand = new SmartFaker<AddAccountingPeriod.Query>().Generate();
        var content = new StringContent(
            content: JsonSerializer.Serialize(addAccountingPeriodCommand),
            Encoding.UTF8,
            mediaType: "application/json"
        );

        // Act
        var response = await _client.PostAsync(requestUri: "/accountingPeriod", content);
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
        var currentGuid = JsonSerializer.Deserialize<Guid>(
            json: await response.Content.ReadAsStringAsync(),
            options
        );

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        if (_client.BaseAddress != null)
        {
            response.Headers.Location.ShouldBe(
                new Uri(
                    uriString: $"/accountingPeriod/{expectedGuid}",
                    UriKind.Relative
                )
            );
        }

        currentGuid.ShouldBe(expectedGuid);
        _addAccountingPeriodMock.Verify(
            m => m.Handle(It.IsAny<AddAccountingPeriod.Query>(), default),
            Times.Once
        );
    }
}
