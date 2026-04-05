using Microsoft.AspNetCore.Mvc;

namespace Kaesseli.Features.Accounts;

public static class AccountApiExtensions
{
    extension(IEndpointRouteBuilder app)
    {
        // ReSharper disable once UnusedMethodReturnValue.Global
        public IEndpointRouteBuilder MapAccountEndpoints()
        {
            app.MapGet(
                pattern: "/account",
                async (GetAccounts.IHandler handler, [FromQuery] AccountType? accountType) =>
                    await handler.Handle(request: new GetAccounts.Query(accountType), default)
            );

            app.MapGet(
                pattern: "/accountingPeriod",
                async (GetAccountingPeriods.IHandler handler) =>
                    await handler.Handle(request: new GetAccountingPeriods.Query(), default)
            );

            app.MapGet(
                pattern: "/accountingPeriod/{accountingPeriodId}/account/{accountId}",
                async (GetAccount.IHandler handler, Guid accountId, Guid accountingPeriodId) =>
                    await handler.Handle(
                        request: new GetAccount.Query(
                            AccountId: accountId,
                            AccountingPeriodId: accountingPeriodId
                        ),
                        default
                    )
            );

            app.MapGet(
                pattern: "/accountingPeriod/{accountingPeriodId}/accountSummary",
                async (GetAccountsSummary.IHandler handler, Guid accountingPeriodId) =>
                    await handler.Handle(
                        request: new GetAccountsSummary.Query(accountingPeriodId),
                        default
                    )
            );

            app.MapGet(
                pattern: "/accountingPeriod/{accountingPeriodId}/overView",
                async (GetFinancialOverview.IHandler handler, Guid accountingPeriodId) =>
                    await handler.Handle(
                        request: new GetFinancialOverview.Query(accountingPeriodId),
                        default
                    )
            );

            app.MapPost(
                pattern: "/account",
                async (AddAccount.IHandler handler, AddAccount.Query command) =>
                {
                    var guid = await handler.Handle(command, default);
                    return Results.Created(uri: $"/account/{guid}", guid);
                }
            );

            app.MapPost(
                pattern: "/accountingPeriod",
                async (AddAccountingPeriod.IHandler handler, AddAccountingPeriod.Query command) =>
                {
                    var guid = await handler.Handle(command, default);
                    return Results.Created(uri: $"/accountingPeriod/{guid}", guid);
                }
            );

            return app;
        }
    }
}
