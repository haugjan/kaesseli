using Kaesseli.Application.Budget;
using MediatR;

namespace Kaesseli.Server.Budget;

public static class BudgetApiExtensions
{
    public static IEndpointRouteBuilder MapBudgetEndpoints(this IEndpointRouteBuilder app)
    {
        return MapBudgetAddBudgetEntryEndpoint(app);
    }

    private static IEndpointRouteBuilder MapBudgetAddBudgetEntryEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/budgetEntry",
            async (IMediator mediator, AddBudgetEntryCommand command) =>
            {
                var guid = await mediator.Send(command);
                return Results.Created($"/budgetEntry/{guid}", guid);
            });
        app.MapGet("/budgetEntry",
            async (IMediator mediator, Guid? accountId, DateOnly? from, DateOnly? to) =>
                await mediator.Send(request: new GetBudgetEntriesQuery
                {
                    AccountId = accountId,
                    FromDate = from,
                    ToDate = to
                }));
        app.MapPost("/account",
                    async (IMediator mediator, AddAccountCommand command) =>
                    {
                        var guid = await mediator.Send(command);
                        return Results.Created(uri: $"/account/{guid}", guid);
                    });
        return app;
    }
}