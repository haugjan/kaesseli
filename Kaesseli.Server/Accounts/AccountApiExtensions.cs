using Kaesseli.Application.Accounts;
using MediatR;

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
            async (IMediator mediator) =>
                await mediator.Send(request: new GetAccountsQuery()));

        app.MapGet(
            pattern: "/account/{accountId}",
            async (IMediator mediator, Guid accountId) =>
                await mediator.Send(request: new GetAccountQuery {AccountId = accountId}));


        app.MapGet(
            pattern: "/accountSummary",
            async (IMediator mediator) =>
                await mediator.Send(request: new GetAccountsSummaryQuery()));

        app.MapPost(
            pattern: "/account",
            async (IMediator mediator, AddAccountCommand command) =>
            {
                var guid = await mediator.Send(command);
                return Results.Created(uri: $"/account/{guid}", guid);
            });
        return app;
    }
}