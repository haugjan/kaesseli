using Kaesseli.Application.Integration.Camt;
using Kaesseli.Application.Integration.NextOpenTransaction;
using Kaesseli.Application.Integration.TransactionQuery;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Kaesseli.Server.Integration;

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

        app.MapPost(
               pattern: "/camt/upload",
               async (IMediator mediator, IFormFile file, [FromForm] Guid accountId) =>
               {
                   await using var stream = file.OpenReadStream();
                   var command = new ProcessCamtFileCommand { Content = stream, AccountId = accountId };

                   return await mediator.Send(command);
               })
           .Accepts<IFormFile>(contentType: "multipart/form-data")
           .DisableAntiforgery();
        return app;
    }
}