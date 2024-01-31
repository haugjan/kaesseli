using Kaesseli.Application.Budget;
using MediatR;

namespace Kaesseli.Server.Budget;

public static class BudgetApiExtensions
{
    public static WebApplication MapBudgetEndpoints(this WebApplication app) => 
        MapBudgetAddBudgetEntryEndpoint(app);

    private static WebApplication MapBudgetAddBudgetEntryEndpoint(WebApplication app)
    {
        app.MapPost("/budgetEntry",
            async (IMediator mediator, AddBudgetEntryCommand command) =>
            {
                var guid = await mediator.Send(command);
                return Results.Created($"/budgetEntry/{guid}", guid);
            });
        return app;
    }
}