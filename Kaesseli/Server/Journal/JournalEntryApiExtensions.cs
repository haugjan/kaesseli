using Kaesseli.Application.Journal;
using Kaesseli.Domain.Accounts;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Routing;


public static class JournalEntryApiExtensions
{
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static IEndpointRouteBuilder MapJournalEndpoints(this IEndpointRouteBuilder app) =>
        MapJournalAddJournalEntryEndpoint(app);

    private static IEndpointRouteBuilder MapJournalAddJournalEntryEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
            pattern: "/journalEntry",
            async (IAddJournalEntryCommandHandler handler, AddJournalEntryCommand command) =>
            {
                var guid = await handler.Handle(command, default);
                return Results.Created(uri: $"/journalEntry/{guid}", guid);
            });
        app.MapGet(
            pattern: "/journalEntry",
            async (
                    IGetJournalEntriesQueryHandler handler,
                    Guid accountingPeriodId,
                    Guid? accountId,
                    AccountType? accountType) =>
                await handler.Handle(
                    request: new GetJournalEntriesQuery
                    {
                        AccountingPeriodId = accountingPeriodId,
                        AccountId = accountId,
                        AccountType = accountType
                    }, default));
        return app;
    }
}
