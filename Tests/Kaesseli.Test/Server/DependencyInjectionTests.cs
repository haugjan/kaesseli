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
            typeof(AddAccount.IHandler),
            typeof(AddAccountingPeriod.IHandler),
            typeof(GetAccount.IHandler),
            typeof(GetAccounts.IHandler),
            typeof(GetAccountingPeriods.IHandler),
            typeof(GetAccountsSummary.IHandler),
            typeof(GetFinancialOverview.IHandler),
            typeof(AddAutomation.IHandler),
            typeof(ApplyAllAutomations.IHandler),
            typeof(GetNrOfPossibleAutomation.IHandler),
            typeof(SetBudget.IHandler),
            typeof(GetBudgetEntries.IHandler),
            typeof(AddJournalEntry.IHandler),
            typeof(GetJournalEntries.IHandler),
            typeof(ProcessFile.IHandler),
            typeof(ProcessCamtFile.IHandler),
            typeof(ProcessPostFinanceCsv.IHandler),
            typeof(OpenTransactionAmountChanged.IHandler),
            typeof(AssignOpenTransaction.IHandler),
            typeof(SplitOpenTransaction.IHandler),
            typeof(GetNextOpenTransaction.IHandler),
            typeof(GetTotalOpenTransaction.IHandler),
            typeof(GetTransactions.IHandler),
            typeof(GetTransactionSummaries.IHandler),
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
