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
                async (SetBudget.IHandler handler, SetBudget.Query command) =>
                {
                    var guid = await handler.Handle(command, default);
                    return Results.Created(uri: $"/budgetEntry/{guid}", guid);
                }
            );

            app.MapGet(
                pattern: "/budgetEntry",
                async (
                        GetBudgetEntries.IHandler handler,
                        Guid accountingPeriodId,
                        Guid? accountId,
                        AccountType? accountType
                        ) =>
                    await handler.Handle(query: new GetBudgetEntries.Query(
                        AccountId: accountId,
                        AccountType: accountType,
                        AccountingPeriodId: accountingPeriodId), default)
            );

            return app;
        }
    }
}
