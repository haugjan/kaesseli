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
    public async Task GetAccountsEndpoint_ShouldReturnAccounts()
    {
        // Arrange
        var accounts = new SmartFaker<GetAccountsQueryResult>().Generate(count: 3);
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetAccountsQuery>(), default)).ReturnsAsync(accounts);

        // Act
        var response = await _client.GetAsync(requestUri: "/account");
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        var currentAccounts =
            JsonSerializer.Deserialize<IEnumerable<GetAccountsQueryResult>>(json: await response.Content.ReadAsStringAsync(), options);


        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        currentAccounts.Should().BeEquivalentTo(accounts);
        _mediatorMock.Verify(m => m.Send(It.IsAny<GetAccountsQuery>(), default), Times.Once);
    }

    [Fact]
    public async Task GetAccountingPeriodsEndpoint_ShouldReturnAccounts()
    {
        // Arrange
        var expectedPeriods = new SmartFaker<GetAccountingPeriodsQueryResult>().Generate(count: 3);
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetAccountingPeriodsQuery>(), default)).ReturnsAsync(expectedPeriods);

        // Act
        var response = await _client.GetAsync(requestUri: "/accountingPeriod");
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        var currentPeriods =
            JsonSerializer.Deserialize<IEnumerable<GetAccountingPeriodsQueryResult>>(json: await response.Content.ReadAsStringAsync(), options);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        currentPeriods.Should().BeEquivalentTo(expectedPeriods);
        _mediatorMock.Verify(m => m.Send(It.IsAny<GetAccountingPeriodsQuery>(), default), Times.Once);
    }

    [Fact]
    public async Task GetAccountEndpoint_ShouldReturnAccounts()
    {
        // Arrange
        var periodId = Guid.NewGuid();
        var accounts = new SmartFaker<GetAccountsQueryResult>().Generate(count: 3);
        var expectedAccount = accounts[index: 1];
        _mediatorMock.Setup(m => m.Send(It.Is<GetAccountQuery>(x=> x.AccountId==expectedAccount.Id), default))
                     .ReturnsAsync((GetAccountQuery _, CancellationToken _) => new GetAccountQueryResult
                     {
                         Id = expectedAccount.Id,
                         Name = expectedAccount.Name,
                         Icon = expectedAccount.Icon,
                         IconColor = expectedAccount.IconColor,
                         Type = expectedAccount.Type,
                         TypeId = expectedAccount.TypeId,
                         AccountBalance = 10,
                         Budget = 11,
                         BudgetBalance = 12,
                         Entries = Array.Empty<GetAccountQueryResultEntry>(),
                         CurrentBudget = 13,
                         BudgetPerMonth = null,
                         BudgetPerYear = null
                     });

        // Act
        var response = await _client.GetAsync(requestUri: $"/accountingPeriod/{periodId}/account/{expectedAccount.Id}");
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }; 

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var accountResponse = JsonSerializer.Deserialize<GetAccountQueryResult>(json: await response.Content.ReadAsStringAsync(), options);
        accountResponse.Should().BeEquivalentTo(expectedAccount);
        _mediatorMock.Verify(m => m.Send(It.Is<GetAccountQuery>(query => query.AccountId == expectedAccount.Id 
                                                                         && query.AccountingPeriodId == periodId), default), Times.Once);

    }

    [Fact]
    public async Task AddAccountEndpoint_ShouldReturnCreatedResult()
    {
        // Arrange
        var guid = Guid.NewGuid();
        _mediatorMock.Setup(m => m.Send(It.IsAny<AddAccountCommand>(), default)).ReturnsAsync(guid);

        var addAccountCommand = new SmartFaker<AddAccountCommand>().Generate();
        var content = new StringContent(
            content: JsonSerializer.Serialize(addAccountCommand),
            Encoding.UTF8,
            mediaType: "application/json");

        // Act
        var response = await _client.PostAsync(requestUri: "/account", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        if (_client.BaseAddress != null)
        {
            response.Headers.Location.Should()
                    .BeEquivalentTo(expectation: new Uri(uriString: $"/account/{guid}", UriKind.Relative));
        }

        _mediatorMock.Verify(m => m.Send(It.IsAny<AddAccountCommand>(), default), Times.Once);
    }

    [Fact]
    public async Task AddAccountingPeriodEndpoint_ShouldReturnCreatedResult()
    {
        // Arrange
        var expectedGuid = Guid.NewGuid();
        _mediatorMock.Setup(m => m.Send(It.IsAny<AddAccountingPeriodCommand>(), default)).ReturnsAsync(expectedGuid);

        var addAccountingPeriodCommand = new SmartFaker<AddAccountingPeriodCommand>().Generate();
        var content = new StringContent(
            content: JsonSerializer.Serialize(addAccountingPeriodCommand),
            Encoding.UTF8,
            mediaType: "application/json");

        // Act
        var response = await _client.PostAsync(requestUri: "/accountingPeriod", content);
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        var currentGuid =
            JsonSerializer.Deserialize<Guid>(json: await response.Content.ReadAsStringAsync(), options);


        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        if (_client.BaseAddress != null)
        {
            response.Headers.Location.Should()
                    .BeEquivalentTo(expectation: new Uri(uriString: $"/accountingPeriod/{expectedGuid}", UriKind.Relative));
        }

        currentGuid.Should().Be(expectedGuid);
        _mediatorMock.Verify(m => m.Send(It.IsAny<AddAccountingPeriodCommand>(), default), Times.Once);
    }
}