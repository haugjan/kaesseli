using Kaesseli.Application.Journal;
using Kaesseli.Domain.Accounts;
using MediatR;

namespace Kaesseli.Server.Journal;

public static class IntegrationApiExtensions
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
                    Guid? debitAccountId,
                    Guid? creditAccountId,
                    DateOnly? from,
                    DateOnly? to,
                    AccountType? accountType) =>
                await mediator.Send(
                    request: new GetJournalEntriesQuery
                    {
                        DebitAccountId = debitAccountId,
                        CreditAccountId = creditAccountId,
                        FromDate = from,
                        ToDate = to,
                        AccountType = accountType
                    }));
        return app;
    }
}