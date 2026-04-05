using Kaesseli.Features.Journal;
using Kaesseli.Features.Accounts;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Routing;

public static class JournalEntryApi
{
    extension(IEndpointRouteBuilder app)
    {
        // ReSharper disable once UnusedMethodReturnValue.Global
        public IEndpointRouteBuilder MapJournalEndpoints()
        {
            app.MapPost(
                pattern: "/journalEntry",
                async (AddJournalEntry.IHandler handler, AddJournalEntry.Query command) =>
                {
                    var guid = await handler.Handle(command, default);
                    return Results.Created(uri: $"/journalEntry/{guid}", guid);
                }
            );

            app.MapGet(
                pattern: "/journalEntry",
                async (
                        GetJournalEntries.IHandler handler,
                        Guid accountingPeriodId,
                        Guid? accountId,
                        AccountType? accountType) =>
                    await handler.Handle(
                        request: new GetJournalEntries.Query(
                            AccountingPeriodId: accountingPeriodId,
                            AccountId: accountId,
                            AccountType: accountType), default)
            );

            return app;
        }
    }
}
