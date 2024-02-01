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
                await mediator.Send(new GetBudgetEntriesQuery
                {
                    AccountId = accountId,
                    FromDate = from,
                    ToDate = to
                }));
        return app;
    }
}