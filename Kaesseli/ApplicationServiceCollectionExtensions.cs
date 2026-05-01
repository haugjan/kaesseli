using Kaesseli.Features.Accounts;
using Kaesseli.Features.Automation;
using Kaesseli.Features.Budget;
using Kaesseli.Features.Integration.FileImport;
using Kaesseli.Features.Integration.NextOpenTransaction;
using Kaesseli.Features.Integration.TransactionQuery;
using Kaesseli.Features.Journal;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        // ReSharper disable once UnusedMethodReturnValue.Global
        public IServiceCollection AddApplicationServices() =>
            services
                .AddSingleton(TimeProvider.System)
                // Accounts
                .AddTransient<AddAccount.IHandler, AddAccount.Handler>()
                .AddTransient<UpdateAccount.IHandler, UpdateAccount.Handler>()
                .AddTransient<DeleteAccount.IHandler, DeleteAccount.Handler>()
                .AddTransient<AddAccountingPeriod.IHandler, AddAccountingPeriod.Handler>()
                .AddTransient<UpdateAccountingPeriod.IHandler, UpdateAccountingPeriod.Handler>()
                .AddTransient<DeleteAccountingPeriod.IHandler, DeleteAccountingPeriod.Handler>()
                .AddTransient<GetAccount.IHandler, GetAccount.Handler>()
                .AddTransient<GetAccounts.IHandler, GetAccounts.Handler>()
                .AddTransient<GetAccountingPeriods.IHandler, GetAccountingPeriods.Handler>()
                .AddTransient<GetAccountsSummary.IHandler, GetAccountsSummary.Handler>()
                .AddTransient<GetFinancialOverview.IHandler, GetFinancialOverview.Handler>()
                .AddTransient<ExportAccountPlan.IHandler, ExportAccountPlan.Handler>()
                .AddTransient<ImportAccountPlan.IHandler, ImportAccountPlan.Handler>()
                .AddTransient<
                    CleanupOrphanedAccountReferences.IHandler,
                    CleanupOrphanedAccountReferences.Handler
                >()
                // Automation
                .AddTransient<AddAutomation.IHandler, AddAutomation.Handler>()
                .AddTransient<ApplyAllAutomations.IHandler, ApplyAllAutomations.Handler>()
                .AddTransient<
                    GetNrOfPossibleAutomation.IHandler,
                    GetNrOfPossibleAutomation.Handler
                >()
                // Budget
                .AddTransient<SetBudget.IHandler, SetBudget.Handler>()
                .AddTransient<GetBudgetEntries.IHandler, GetBudgetEntries.Handler>()
                // Journal
                .AddTransient<AddJournalEntry.IHandler, AddJournalEntry.Handler>()
                .AddTransient<AddOpeningBalance.IHandler, AddOpeningBalance.Handler>()
                .AddTransient<GetJournalEntries.IHandler, GetJournalEntries.Handler>()
                // Integration - FileImport
                .AddTransient<ProcessFile.IHandler, ProcessFile.Handler>()
                .AddTransient<ProcessCamtFile.IHandler, ProcessCamtFile.Handler>()
                .AddTransient<ProcessPostFinanceCsv.IHandler, ProcessPostFinanceCsv.Handler>()
                // Integration - NextOpenTransaction
                .AddTransient<
                    OpenTransactionAmountChanged.IHandler,
                    OpenTransactionAmountChanged.Handler
                >()
                .AddTransient<AssignOpenTransaction.IHandler, AssignOpenTransaction.Handler>()
                .AddTransient<SplitOpenTransaction.IHandler, SplitOpenTransaction.Handler>()
                .AddTransient<GetNextOpenTransaction.IHandler, GetNextOpenTransaction.Handler>()
                .AddTransient<GetTotalOpenTransaction.IHandler, GetTotalOpenTransaction.Handler>()
                // Integration - TransactionQuery
                .AddTransient<GetTransactions.IHandler, GetTransactions.Handler>()
                .AddTransient<GetTransactionSummaries.IHandler, GetTransactionSummaries.Handler>();
    }
}
