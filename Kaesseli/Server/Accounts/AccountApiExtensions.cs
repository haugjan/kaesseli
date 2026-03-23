using Kaesseli.Application.Accounts;
using Kaesseli.Domain.Accounts;
using Microsoft.AspNetCore.Mvc;

namespace Kaesseli.Server.Accounts;

public static class AccountApiExtensions
{
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static IEndpointRouteBuilder MapAccountEndpoints(this IEndpointRouteBuilder app) =>
        MapAddAccountEndpoint(app);

    private static IEndpointRouteBuilder MapAddAccountEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet(
            pattern: "/account",
            async (GetAccounts.IHandler handler, [FromQuery] AccountType? accountType) =>
                await handler.Handle(request: new GetAccounts.Query { AccountType = accountType }, default));

        app.MapGet(
            pattern: "/accountingPeriod",
            async (GetAccountingPeriods.IHandler handler) =>
                await handler.Handle(request: new GetAccountingPeriods.Query(), default));

        app.MapGet(
            pattern: "/accountingPeriod/{accountingPeriodId}/account/{accountId}",
            async (GetAccount.IHandler handler, Guid accountId, Guid accountingPeriodId) =>
                await handler.Handle(request: new GetAccount.Query
                {
                    AccountId = accountId,
                    AccountingPeriodId = accountingPeriodId
                }, default));

        app.MapGet(
            pattern: "/accountingPeriod/{accountingPeriodId}/accountSummary",
            async (GetAccountsSummary.IHandler handler, Guid accountingPeriodId) =>
                await handler.Handle(request: new GetAccountsSummary.Query
                {
                    AccountingPeriodId = accountingPeriodId
                }, default));

        app.MapGet(
            pattern: "/accountingPeriod/{accountingPeriodId}/overView",
            async (GetFinancialOverview.IHandler handler, Guid accountingPeriodId) =>
                await handler.Handle(request: new GetFinancialOverview.Query
                {
                    AccountingPeriodId = accountingPeriodId
                }, default));

        app.MapPost(
            pattern: "/account",
            async (AddAccount.IHandler handler, AddAccount.Query command) =>
            {
                var guid = await handler.Handle(command, default);
                return Results.Created(uri: $"/account/{guid}", guid);
            });

        app.MapPost(
            pattern: "/accountingPeriod",
            async (AddAccountingPeriod.IHandler handler, AddAccountingPeriod.Query command) =>
            {
                var guid = await handler.Handle(command, default);
                return Results.Created(uri: $"/accountingPeriod/{guid}", guid);
            });

        return app;
    }
}
