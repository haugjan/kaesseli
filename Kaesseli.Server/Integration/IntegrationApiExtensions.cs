using Kaesseli.Application.Integration.FileImport;
using Kaesseli.Application.Integration.NextOpenTransaction;
using Kaesseli.Application.Integration.TransactionQuery;
using Kaesseli.Domain.Accounts;
using Kaesseli.Domain.Integration;
using MediatR;
using Microsoft.AspNetCore.Mvc;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Routing;

public static class IntegrationApiExtensions
{
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static IEndpointRouteBuilder MapIntegrationEndpoints(this IEndpointRouteBuilder app) =>
        MapCamtApi(app);

    private static IEndpointRouteBuilder MapCamtApi(IEndpointRouteBuilder app)
    {
        app.MapGet(
            pattern: "/transactionSummary",
            async (IMediator mediator) =>
            {
                var query = new GetTransactionSummariesQuery();
                return await mediator.Send(query);
            });
        app.MapGet(
            pattern: "/transaction",
            async (IMediator mediator, [FromQuery] Guid transactionSummaryId) =>
            {
                var query = new GetTransactionsQuery { TransactionSummaryId = transactionSummaryId };
                return await mediator.Send(query);
            });

        app.MapGet(
            pattern: "/transaction/nextOpen",
            async (IMediator mediator, [FromQuery] int? skip) =>
            {
                var query = new GetNextOpenTransactionQuery { Skip = skip.GetValueOrDefault() };
                return await mediator.Send(query);
            });
        app.MapGet(
            pattern: "/transaction/totalOpen",
            async (IMediator mediator, [FromQuery] int? skip) =>
            {
                var query = new GetTotalOpenTransactionQuery();
                return await mediator.Send(query);
            });



        app.MapPatch(
            pattern: "/transaction/journalEntry",
            async (IMediator mediator, [FromBody] AssignOpenTransactionCommand cmd) =>
            {
                await mediator.Send(cmd);
            });

        app.MapPatch(
            pattern: "/transaction/journalEntry/split",
            async (IMediator mediator, [FromBody] SplitOpenTransactionCommand cmd) =>
            {
                await mediator.Send(cmd);
            });

        app.MapPost(
               pattern: "/file/upload",
               async (IMediator mediator, IFormFile file, [FromForm] Guid accountId, [FromQuery] Guid accountingPeriodId) =>
               {
                   await using var stream = file.OpenReadStream();
                   var command = new ProcessFileCommand
                   {
                       Content = stream,
                       AccountId = accountId,
                       FileType = file.FileName.EndsWith(".csv")
                                      ? FileType.PostFinanceCsv
                                      : FileType.Camt,
                       AccountingPeriodId = accountingPeriodId
                   };

                   return await mediator.Send(command);
               })
           .Accepts<IFormFile>(contentType: "multipart/form-data")
           .DisableAntiforgery();
        return app;
    }
}