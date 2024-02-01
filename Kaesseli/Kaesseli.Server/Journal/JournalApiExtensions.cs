using Kaesseli.Application.Journal;
using MediatR;

namespace Kaesseli.Server.Journal;

public static class JournalApiExtensions
{
    public static IEndpointRouteBuilder MapJournalEndpoints(this IEndpointRouteBuilder app) => 
        MapJournalAddJournalEntryEndpoint(app);

    private static IEndpointRouteBuilder MapJournalAddJournalEntryEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/JournalEntry",
            async (IMediator mediator, AddJournalEntryCommand command) =>
            {
                var guid = await mediator.Send(command);
                return Results.Created($"/JournalEntry/{guid}", guid);
            });
        return app;
    }
}