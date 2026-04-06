using Kaesseli.Features.Accounts;
using Kaesseli.Features.Automation;
using Kaesseli.Features.Budget;
using Kaesseli.Features.Integration.FileImport;
using Kaesseli.Features.Integration.NextOpenTransaction;
using Kaesseli.Features.Integration.TransactionQuery;
using Kaesseli.Features.Journal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Kaesseli.Test.Infrastructure;

public class DependencyInjectionTests
{
    [Fact]
    public void ServiceProvider_CanResolveAllHandlers()
    {
        // Arrange
        var configMock = Substitute.For<IConfiguration>();
        var configSectionMock = Substitute.For<IConfigurationSection>();
        configMock
            .GetSection(Arg.Any<string>())
            .Returns(configSectionMock);
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddApplicationServices().AddInfrastructureServices(configMock);

        var serviceProvider = serviceCollection.BuildServiceProvider();

        var handlerInterfaces = new[]
        {
            typeof(AddAccount.IHandler),
            typeof(AddAccountingPeriod.IHandler),
            typeof(UpdateAccountingPeriod.IHandler),
            typeof(DeleteAccountingPeriod.IHandler),
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
            typeof(AddOpeningBalance.IHandler),
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
            handler.ShouldNotBeNull(
                $"weil {handlerInterface.Name} im DI Container registriert und auflösbar sein sollte."
            );
        }
    }
}
