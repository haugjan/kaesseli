using FluentAssertions;
using Kaesseli.Application.Accounts;
using Kaesseli.Application.Automation;
using Kaesseli.Application.Budget;
using Kaesseli.Application.Integration.FileImport;
using Kaesseli.Application.Integration.NextOpenTransaction;
using Kaesseli.Application.Integration.TransactionQuery;
using Kaesseli.Application.Journal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Kaesseli.Server.Test;

public class DependencyInjectionTests
{
    [Fact]
    public void ServiceProvider_CanResolveAllHandlers()
    {
        // Arrange
        var configMock = new Mock<IConfiguration>();
        var configSectionMock = new Mock<IConfigurationSection>();
        configMock.Setup(cfg => cfg.GetSection(It.IsAny<string>()))
                  .Returns((string _) => configSectionMock.Object);
        var serviceCollection = new ServiceCollection();
        serviceCollection
            .AddApplicationServices()
            .AddInfrastructureServices(configMock.Object);

        var serviceProvider = serviceCollection.BuildServiceProvider();

        var handlerInterfaces = new[]
        {
            typeof(IAddAccountCommandHandler),
            typeof(IAddAccountingPeriodCommandHandler),
            typeof(IGetAccountQueryHandler),
            typeof(IGetAccountsQueryHandler),
            typeof(IGetAccountingPeriodsQueryHandler),
            typeof(IGetAccountsSummaryQueryHandler),
            typeof(IGetFinancialOverviewCommandHandler),
            typeof(IAddAutomationCommandHandler),
            typeof(IApplyAllAutomationsCommandHandler),
            typeof(IGetNrOfPossibleAutomationQueryHandler),
            typeof(ISetBudgetCommandHandler),
            typeof(IGetBudgetEntriesQueryHandler),
            typeof(IAddJournalEntryCommandHandler),
            typeof(IGetJournalEntriesQueryHandler),
            typeof(IProcessFileCommandHandler),
            typeof(IProcessCamtFileCommandHandler),
            typeof(IProcessPostFinanceCsvCommandHandler),
            typeof(IOpenTransactionAmountChangedEventHandler),
            typeof(IAssignOpenTransactionCommandHandler),
            typeof(ISplitOpenTransactionCommandHandler),
            typeof(IGetNextOpenTransactionQueryHandler),
            typeof(IGetTotalOpenTransactionQueryHandler),
            typeof(IGetTransactionsQueryHandler),
            typeof(IGetTransactionSummariesQueryHandler),
        };

        foreach (var handlerInterface in handlerInterfaces)
        {
            // Act
            var handler = serviceProvider.GetService(handlerInterface);

            // Assert
            handler.Should().NotBeNull(because: $"weil {handlerInterface.Name} im DI Container registriert und auflösbar sein sollte.");
        }
    }
}
