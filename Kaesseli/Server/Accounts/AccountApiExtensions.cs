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
            async (IGetAccountsQueryHandler handler, [FromQuery] AccountType? accountType) =>
                await handler.Handle(request: new GetAccountsQuery { AccountType = accountType }, default));

        app.MapGet(
            pattern: "/accountingPeriod",
            async (IGetAccountingPeriodsQueryHandler handler) =>
                await handler.Handle(request: new GetAccountingPeriodsQuery(), default));

        app.MapGet(
            pattern: "/accountingPeriod/{accountingPeriodId}/account/{accountId}",
            async (IGetAccountQueryHandler handler, Guid accountId, Guid accountingPeriodId) =>
                await handler.Handle(request: new GetAccountQuery
                {
                    AccountId = accountId,
                    AccountingPeriodId = accountingPeriodId
                }, default));

        app.MapGet(
            pattern: "/accountingPeriod/{accountingPeriodId}/accountSummary",
            async (IGetAccountsSummaryQueryHandler handler, Guid accountingPeriodId) =>
                await handler.Handle(request: new GetAccountsSummaryQuery
                {
                    AccountingPeriodId = accountingPeriodId
                }, default));

        app.MapGet(
            pattern: "/accountingPeriod/{accountingPeriodId}/overView",
            async (IGetFinancialOverviewCommandHandler handler, Guid accountingPeriodId) =>
                await handler.Handle(request: new GetFinancialOverviewCommand
                {
                    AccountingPeriodId = accountingPeriodId
                }, default));

        app.MapPost(
            pattern: "/account",
            async (IAddAccountCommandHandler handler, AddAccountCommand command) =>
            {
                var guid = await handler.Handle(command, default);
                return Results.Created(uri: $"/account/{guid}", guid);
            });

        app.MapPost(
            pattern: "/accountingPeriod",
            async (IAddAccountingPeriodCommandHandler handler, AddAccountingPeriodCommand command) =>
            {
                var guid = await handler.Handle(command, default);
                return Results.Created(uri: $"/accountingPeriod/{guid}", guid);
            });

        return app;
    }
}
