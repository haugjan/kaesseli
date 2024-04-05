using Kaesseli.Application.Journal;
using Kaesseli.Domain.Accounts;
using MediatR;

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
            async (IMediator mediator, AddJournalEntryCommand command) =>
            {
                var guid = await mediator.Send(command);
                return Results.Created(uri: $"/journalEntry/{guid}", guid);
            });
        app.MapGet(
            pattern: "/journalEntry",
            async (
                    IMediator mediator,
                    Guid accountingPeriodId,
                    Guid? accountId,
                    AccountType? accountType) =>
                await mediator.Send(
                    request: new GetJournalEntriesQuery
                    {
                        AccountingPeriodId = accountingPeriodId,
                        AccountId = accountId,
                        AccountType = accountType
                    }));
        return app;
    }
}