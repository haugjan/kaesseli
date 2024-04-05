using Kaesseli.Application.Budget;
using Kaesseli.Domain.Accounts;
using MediatR;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Routing;

public static class BudgetApiExtensions
{
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static IEndpointRouteBuilder MapBudgetEndpoints(this IEndpointRouteBuilder app) =>
        MapBudgetBudgetEntryEndpoint(app);

    private static IEndpointRouteBuilder MapBudgetBudgetEntryEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
            pattern: "/budgetEntry",
            async (IMediator mediator, SetBudgetCommand command) =>
            {
                var guid = await mediator.Send(command);
                return Results.Created(uri: $"/budgetEntry/{guid}", guid);
            });
        app.MapGet(
            pattern: "/budgetEntry",
            async (
                    IMediator mediator,
                    Guid accountingPeriodId,
                    Guid? accountId,
                    AccountType? accountType
                    ) =>
                await mediator.Send(request: new GetBudgetEntriesQuery
                {
                    AccountId = accountId,
                    AccountType = accountType,
                    AccountingPeriodId = accountingPeriodId
                }));
       
        return app;
    }
}