using Kaesseli.Application.Accounts;
using Kaesseli.Application.Automation;
using Kaesseli.Application.Budget;
using Kaesseli.Application.Integration.FileImport;
using Kaesseli.Application.Integration.NextOpenTransaction;
using Kaesseli.Application.Integration.TransactionQuery;
using Kaesseli.Application.Journal;
using Kaesseli.Application.Utility;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static IServiceCollection AddApplicationServices(this IServiceCollection services) =>
        services
            .AddTransient<IDateTimeService, DateTimeService>()
            .AddTransient<IEnvironmentService, EnvironmentService>()
            // Accounts
            .AddTransient<IAddAccountCommandHandler, AddAccountCommandHandler>()
            .AddTransient<IAddAccountingPeriodCommandHandler, AddAccountingPeriodCommandHandler>()
            .AddTransient<IGetAccountQueryHandler, GetAccountQueryHandler>()
            .AddTransient<IGetAccountsQueryHandler, GetAccountsQueryHandler>()
            .AddTransient<IGetAccountingPeriodsQueryHandler, GetAccountingPeriodsQueryHandler>()
            .AddTransient<IGetAccountsSummaryQueryHandler, GetAccountsSummaryQueryHandler>()
            .AddTransient<IGetFinancialOverviewCommandHandler, GetFinancialOverviewCommandHandler>()
            // Automation
            .AddTransient<IAddAutomationCommandHandler, AddAutomationCommandHandler>()
            .AddTransient<IApplyAllAutomationsCommandHandler, ApplyAllAutomationsCommandHandler>()
            .AddTransient<IGetNrOfPossibleAutomationQueryHandler, GetNrOfPossibleAutomationQueryHandler>()
            // Budget
            .AddTransient<ISetBudgetCommandHandler, SetBudgetCommandHandler>()
            .AddTransient<IGetBudgetEntriesQueryHandler, GetBudgetEntriesQueryHandler>()
            // Journal
            .AddTransient<IAddJournalEntryCommandHandler, AddJournalEntryCommandHandler>()
            .AddTransient<IGetJournalEntriesQueryHandler, GetJournalEntriesQueryHandler>()
            // Integration - FileImport
            .AddTransient<IProcessFileCommandHandler, ProcessFileCommandHandler>()
            .AddTransient<IProcessCamtFileCommandHandler, ProcessCamtFileCommandHandler>()
            .AddTransient<IProcessPostFinanceCsvCommandHandler, ProcessPostFinanceCsvCommandHandler>()
            // Integration - NextOpenTransaction
            .AddTransient<IOpenTransactionAmountChangedEventHandler, OpenTransactionAmountChangedEventHandler>()
            .AddTransient<IAssignOpenTransactionCommandHandler, AssignOpenTransactionCommandHandler>()
            .AddTransient<ISplitOpenTransactionCommandHandler, SplitOpenTransactionCommandHandler>()
            .AddTransient<IGetNextOpenTransactionQueryHandler, GetNextOpenTransactionQueryHandler>()
            .AddTransient<IGetTotalOpenTransactionQueryHandler, GetTotalOpenTransactionQueryHandler>()
            // Integration - TransactionQuery
            .AddTransient<IGetTransactionsQueryHandler, GetTransactionsQueryHandler>()
            .AddTransient<IGetTransactionSummariesQueryHandler, GetTransactionSummariesQueryHandler>();
}
