using Kaesseli.Application.Budget;
using Kaesseli.Domain.Accounts;

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
            async (SetBudget.IHandler handler, SetBudget.Query command) =>
            {
                var guid = await handler.Handle(command, default);
                return Results.Created(uri: $"/budgetEntry/{guid}", guid);
            });
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
                    AccountingPeriodId: accountingPeriodId), default));

        return app;
    }
}
