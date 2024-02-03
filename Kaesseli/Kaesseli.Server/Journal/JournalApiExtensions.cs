using Kaesseli.Application.Journal;
using MediatR;

namespace Kaesseli.Server.Journal;

public static class JournalApiExtensions
{
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
                    Guid? accountId,
                    DateOnly? from,
                    DateOnly? to) =>
                await mediator.Send(request: new GetJournalEntriesQuery { AccountId = accountId, FromDate = from, ToDate = to }));
        app.MapPatch(
            pattern: "/journalEntry",
            async (
                    IMediator mediator,
                    AssignAccountToJournalEntryCommand command) =>
                await mediator.Send(command));

        return app;
    }
}