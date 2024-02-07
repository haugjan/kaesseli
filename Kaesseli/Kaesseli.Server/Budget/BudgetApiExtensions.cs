using Kaesseli.Application.Budget;
using MediatR;

namespace Kaesseli.Server.Budget;

public static class BudgetApiExtensions
{
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static IEndpointRouteBuilder MapBudgetEndpoints(this IEndpointRouteBuilder app) =>
        MapBudgetAddBudgetEntryEndpoint(app);

    private static IEndpointRouteBuilder MapBudgetAddBudgetEntryEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
            pattern: "/budgetEntry",
            async (IMediator mediator, AddBudgetEntryCommand command) =>
            {
                var guid = await mediator.Send(command);
                return Results.Created(uri: $"/budgetEntry/{guid}", guid);
            });
        app.MapGet(
            pattern: "/budgetEntry",
            async (
                    IMediator mediator,
                    Guid? accountId,
                    DateOnly? from,
                    DateOnly? to) =>
                await mediator.Send(request: new GetBudgetEntriesQuery { AccountId = accountId, FromDate = from, ToDate = to }));
       
        return app;
    }
}