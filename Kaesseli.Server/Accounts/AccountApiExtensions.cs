using Kaesseli.Application.Accounts;
using Kaesseli.Domain.Accounts;
using MediatR;
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
            async (IMediator mediator, [FromQuery] AccountType? accountType) =>
                await mediator.Send(request: new GetAccountsQuery{AccountType = accountType}));

        app.MapGet(
            pattern: "/accountingPeriod",
            async (IMediator mediator) =>
                await mediator.Send(request: new GetAccountingPeriodsQuery()));


        app.MapGet(
            pattern: "/accountingPeriod/{accountingPeriodId}/account/{accountId}",
            async (IMediator mediator, Guid accountId, Guid accountingPeriodId) =>
                await mediator.Send(request: new GetAccountQuery
                {
                    AccountId = accountId,
                    AccountingPeriodId = accountingPeriodId
                }));


        app.MapGet(
            pattern: "/accountingPeriod/{accountingPeriodId}/accountSummary",
            async (IMediator mediator, Guid accountingPeriodId) =>
                await mediator.Send(request: new GetAccountsSummaryQuery
                {
                    AccountingPeriodId = accountingPeriodId
                }));

        app.MapPost(
            pattern: "/account",
            async (IMediator mediator, AddAccountCommand command) =>
            {
                var guid = await mediator.Send(command);
                return Results.Created(uri: $"/account/{guid}", guid);
            });

        app.MapPost(
            pattern: "/accountingPeriod",
            async (IMediator mediator, AddAccountingPeriodCommand command) =>
            {
                var guid = await mediator.Send(command);
                return Results.Created(uri: $"/accountingPeriod/{guid}", guid);
            });

        return app;
    }
}