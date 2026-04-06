using System.Net;
using System.Text;
using System.Text.Json;
using Kaesseli.Contracts.Accounts;
using Kaesseli.Features.Accounts;
using Account = Kaesseli.Contracts.Accounts.Account;
using Kaesseli.Test.Faker;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Kaesseli.Test.Features.Accounts;

public class AccountApiTests : IAsyncLifetime
{
    private HttpClient _client = null!;
    private readonly GetAccounts.IHandler _getAccountsMock = Substitute.For<GetAccounts.IHandler>();
    private readonly GetAccountingPeriods.IHandler _getAccountingPeriodsMock = Substitute.For<GetAccountingPeriods.IHandler>();
    private readonly GetAccount.IHandler _getAccountMock = Substitute.For<GetAccount.IHandler>();
    private readonly GetAccountsSummary.IHandler _getAccountsSummaryMock = Substitute.For<GetAccountsSummary.IHandler>();
    private readonly GetFinancialOverview.IHandler _getFinancialOverviewMock = Substitute.For<GetFinancialOverview.IHandler>();
    private readonly AddAccount.IHandler _addAccountMock = Substitute.For<AddAccount.IHandler>();
    private readonly AddAccountingPeriod.IHandler _addAccountingPeriodMock = Substitute.For<AddAccountingPeriod.IHandler>();
    private readonly UpdateAccountingPeriod.IHandler _updateAccountingPeriodMock = Substitute.For<UpdateAccountingPeriod.IHandler>();
    private readonly DeleteAccountingPeriod.IHandler _deleteAccountingPeriodMock = Substitute.For<DeleteAccountingPeriod.IHandler>();
    private readonly UpdateAccount.IHandler _updateAccountMock = Substitute.For<UpdateAccount.IHandler>();
    private readonly DeleteAccount.IHandler _deleteAccountMock = Substitute.For<DeleteAccount.IHandler>();

    public async Task InitializeAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddRouting();
        builder.Services.AddSingleton(_getAccountsMock);
        builder.Services.AddSingleton(_getAccountingPeriodsMock);
        builder.Services.AddSingleton(_getAccountMock);
        builder.Services.AddSingleton(_getAccountsSummaryMock);
        builder.Services.AddSingleton(_getFinancialOverviewMock);
        builder.Services.AddSingleton(_addAccountMock);
        builder.Services.AddSingleton(_addAccountingPeriodMock);
        builder.Services.AddSingleton(_updateAccountingPeriodMock);
        builder.Services.AddSingleton(_deleteAccountingPeriodMock);
        builder.Services.AddSingleton(_updateAccountMock);
        builder.Services.AddSingleton(_deleteAccountMock);

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
        var accounts = new SmartFaker<AccountOverview>().Generate(count: 3);
        _getAccountsSummaryMock
            .Handle(Arg.Any<GetAccountsSummary.Query>(), Arg.Any<CancellationToken>())
            .Returns(accounts);

        // Act
        var response = await _client.GetAsync(
            requestUri: $"/accountingPeriod/{periodId}/accountSummary"
        );

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        await _getAccountsSummaryMock.Received(1)
            .Handle(Arg.Any<GetAccountsSummary.Query>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetAccountEndpoint_ShouldReturnSingleAccount()
    {
        // Arrange
        var periodId = Guid.NewGuid();
        var accounts = new SmartFaker<Account>().Generate(count: 3);
        var expectedAccount = accounts[index: 1];
        _getAccountMock
            .Handle(Arg.Is<GetAccount.Query>(x => x.AccountId == expectedAccount.Id), Arg.Any<CancellationToken>())
            .Returns(
                new AccountStatement(
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
                    Entries: Array.Empty<AccountStatementEntry>())
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
        var accountResponse = JsonSerializer.Deserialize<AccountStatement>(
            json: await response.Content.ReadAsStringAsync(),
            options
        );
        Assert.Equivalent(expectedAccount, accountResponse);
        await _getAccountMock.Received(1)
            .Handle(
                Arg.Is<GetAccount.Query>(query =>
                    query.AccountId == expectedAccount.Id
                    && query.AccountingPeriodId == periodId
                ),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task AddAccountEndpoint_ShouldReturnCreatedResult()
    {
        // Arrange
        var guid = Guid.NewGuid();
        _addAccountMock
            .Handle(Arg.Any<AddAccount.Query>(), Arg.Any<CancellationToken>())
            .Returns(guid);

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

        await _addAccountMock.Received(1).Handle(Arg.Any<AddAccount.Query>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddAccountingPeriodEndpoint_ShouldReturnCreatedResult()
    {
        // Arrange
        var expectedGuid = Guid.NewGuid();
        _addAccountingPeriodMock
            .Handle(Arg.Any<AddAccountingPeriod.Query>(), Arg.Any<CancellationToken>())
            .Returns(expectedGuid);

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
        await _addAccountingPeriodMock.Received(1)
            .Handle(Arg.Any<AddAccountingPeriod.Query>(), Arg.Any<CancellationToken>());
    }
}
