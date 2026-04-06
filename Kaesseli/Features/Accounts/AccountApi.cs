using Microsoft.AspNetCore.Mvc;

namespace Kaesseli.Features.Accounts;

public static class AccountApi
{
    extension(IEndpointRouteBuilder app)
    {
        // ReSharper disable once UnusedMethodReturnValue.Global
        public IEndpointRouteBuilder MapAccountEndpoints()
        {
            app.MapGet(
                pattern: "/account",
                async (GetAccounts.IHandler handler, [FromQuery] AccountType? accountType, CancellationToken ct) =>
                    await handler.Handle(new GetAccounts.Query(accountType), ct)
            );

            app.MapGet(
                pattern: "/accountingPeriod",
                async (GetAccountingPeriods.IHandler handler, CancellationToken ct) =>
                    await handler.Handle(ct)
            );

            app.MapGet(
                pattern: "/accountingPeriod/{accountingPeriodId}/account/{accountId}",
                async (GetAccount.IHandler handler, Guid accountId, Guid accountingPeriodId, CancellationToken ct) =>
                    await handler.Handle(new GetAccount.Query(accountId, accountingPeriodId), ct)
            );

            app.MapGet(
                pattern: "/accountingPeriod/{accountingPeriodId}/accountSummary",
                async (GetAccountsSummary.IHandler handler, Guid accountingPeriodId, CancellationToken ct) =>
                    await handler.Handle(new GetAccountsSummary.Query(accountingPeriodId), ct)
            );

            app.MapGet(
                pattern: "/accountingPeriod/{accountingPeriodId}/overView",
                async (GetFinancialOverview.IHandler handler, Guid accountingPeriodId, CancellationToken ct) =>
                    await handler.Handle(new GetFinancialOverview.Query(accountingPeriodId), ct)
            );

            app.MapPost(
                pattern: "/account",
                async (AddAccount.IHandler handler, AddAccount.Query command, CancellationToken ct) =>
                {
                    var guid = await handler.Handle(command, ct);
                    return Results.Created(uri: $"/account/{guid}", guid);
                }
            );

            app.MapPost(
                pattern: "/accountingPeriod",
                async (AddAccountingPeriod.IHandler handler, AddAccountingPeriod.Query command, CancellationToken ct) =>
                {
                    var guid = await handler.Handle(command, ct);
                    return Results.Created(uri: $"/accountingPeriod/{guid}", guid);
                }
            );

            app.MapPut(
                pattern: "/accountingPeriod/{id}",
                async (UpdateAccountingPeriod.IHandler handler, Guid id, UpdateAccountingPeriod.Query command) =>
                {
                    await handler.Handle(command with { Id = id }, default);
                    return Results.NoContent();
                }
            );

            app.MapDelete(
                pattern: "/accountingPeriod/{id}",
                async (DeleteAccountingPeriod.IHandler handler, Guid id) =>
                {
                    await handler.Handle(new DeleteAccountingPeriod.Query(id), default);
                    return Results.NoContent();
                }
            );

            app.MapPut(
                pattern: "/account/{id}",
                async (UpdateAccount.IHandler handler, Guid id, UpdateAccount.Query command) =>
                {
                    await handler.Handle(command with { Id = id }, default);
                    return Results.NoContent();
                }
            );

            app.MapDelete(
                pattern: "/account/{id}",
                async (DeleteAccount.IHandler handler, Guid id) =>
                {
                    await handler.Handle(new DeleteAccount.Query(id), default);
                    return Results.NoContent();
                }
            );

            return app;
        }
    }
}
