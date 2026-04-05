using Kaesseli.Features.Budget;
using Kaesseli.Features.Accounts;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Routing;

public static class BudgetApi
{
    extension(IEndpointRouteBuilder app)
    {
        // ReSharper disable once UnusedMethodReturnValue.Global
        public IEndpointRouteBuilder MapBudgetEndpoints()
        {
            app.MapPost(
                pattern: "/budgetEntry",
                async (SetBudget.IHandler handler, SetBudget.Query command, CancellationToken ct) =>
                {
                    var guid = await handler.Handle(command, ct);
                    return Results.Created(uri: $"/budgetEntry/{guid}", guid);
                }
            );

            app.MapGet(
                pattern: "/budgetEntry",
                async (
                        GetBudgetEntries.IHandler handler,
                        Guid accountingPeriodId,
                        Guid? accountId,
                        AccountType? accountType,
                        CancellationToken ct
                        ) =>
                    await handler.Handle(new GetBudgetEntries.Query(accountId, accountType, accountingPeriodId), ct)
            );

            return app;
        }
    }
}
