using Kaesseli.Application.Journal;
using Kaesseli.Domain.Accounts;
using MediatR;

namespace Kaesseli.Server.Integration;

public static class IntegrationApiExtensions
{
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static IEndpointRouteBuilder MapJournalEndpoints(this IEndpointRouteBuilder app) =>
        MapJournalAddJournalEntryEndpoint(app);

    private static IEndpointRouteBuilder MapJournalAddJournalEntryEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
            pattern: "/import",
            async (IMediator mediator, AddJournalEntryCommand command) =>
            {
                var guid = await mediator.Send(command);
                return Results.Created(uri: $"/journalEntry/{guid}", guid);
            });
        return app;
    }
}