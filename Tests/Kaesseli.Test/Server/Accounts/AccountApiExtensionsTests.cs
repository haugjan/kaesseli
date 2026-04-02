using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Kaesseli.Application.Accounts;
using Kaesseli.Server.Accounts;
using Kaesseli.Test.Faker;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Kaesseli.Test.Server.Accounts;

public class AccountApiExtensionsTests
{
    private readonly HttpClient _client;
    private readonly Mock<GetAccounts.IHandler> _getAccountsMock = new();
    private readonly Mock<GetAccountingPeriods.IHandler> _getAccountingPeriodsMock = new();
    private readonly Mock<GetAccount.IHandler> _getAccountMock = new();
    private readonly Mock<GetAccountsSummary.IHandler> _getAccountsSummaryMock = new();
    private readonly Mock<GetFinancialOverview.IHandler> _getFinancialOverviewMock = new();
    private readonly Mock<AddAccount.IHandler> _addAccountMock = new();
    private readonly Mock<AddAccountingPeriod.IHandler> _addAccountingPeriodMock = new();

    public AccountApiExtensionsTests()
    {
        var server = new TestServer(
            builder: new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddRouting();
                    services.AddSingleton(_getAccountsMock.Object);
                    services.AddSingleton(_getAccountingPeriodsMock.Object);
                    services.AddSingleton(_getAccountMock.Object);
                    services.AddSingleton(_getAccountsSummaryMock.Object);
                    services.AddSingleton(_getFinancialOverviewMock.Object);
                    services.AddSingleton(_addAccountMock.Object);
                    services.AddSingleton(_addAccountingPeriodMock.Object);
                })
                .Configure(app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapAccountEndpoints();
                    });
                })
        );

        _client = server.CreateClient();
    }

    [Fact]
    public async Task GetAccountsEndpoint_ShouldReturnAccounts()
    {
        // Arrange
        var periodId = Guid.NewGuid();
        var accounts = new SmartFaker<GetAccountsSummary.Result>().Generate(count: 3);
        _getAccountsSummaryMock
            .Setup(m => m.Handle(It.IsAny<GetAccountsSummary.Query>(), default))
            .ReturnsAsync(accounts);

        // Act
        var response = await _client.GetAsync(
            requestUri: $"/accountingPeriod/{periodId}/accountSummary"
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
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
        var accounts = new SmartFaker<GetAccounts.Result>().Generate(count: 3);
        var expectedAccount = accounts[index: 1];
        _getAccountMock
            .Setup(m =>
                m.Handle(It.Is<GetAccount.Query>(x => x.AccountId == expectedAccount.Id), default)
            )
            .ReturnsAsync(
                (GetAccount.Query _, CancellationToken _) =>
                    new GetAccount.Result(
                        Id: expectedAccount.Id,
                        Name: expectedAccount.Name,
                        Icon: expectedAccount.Icon,
                        IconColor: expectedAccount.IconColor,
                        Type: expectedAccount.Type,
                        TypeId: expectedAccount.TypeId,
                        AccountBalance: 10,
                        Budget: 11,
                        BudgetPerMonth: null,
                        BudgetPerYear: null,
                        CurrentBudget: 13,
                        BudgetBalance: 12,
                        Entries: Array.Empty<GetAccount.ResultEntry>())
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
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var accountResponse = JsonSerializer.Deserialize<GetAccount.Result>(
            json: await response.Content.ReadAsStringAsync(),
            options
        );
        accountResponse.Should().BeEquivalentTo(expectedAccount);
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
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        if (_client.BaseAddress != null)
        {
            response
                .Headers.Location.Should()
                .BeEquivalentTo(
                    expectation: new Uri(uriString: $"/account/{guid}", UriKind.Relative)
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
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        if (_client.BaseAddress != null)
        {
            response
                .Headers.Location.Should()
                .BeEquivalentTo(
                    expectation: new Uri(
                        uriString: $"/accountingPeriod/{expectedGuid}",
                        UriKind.Relative
                    )
                );
        }

        currentGuid.Should().Be(expectedGuid);
        _addAccountingPeriodMock.Verify(
            m => m.Handle(It.IsAny<AddAccountingPeriod.Query>(), default),
            Times.Once
        );
    }
}
