using System.IO;
using System.IO.Compression;
using Kaesseli.Application.Integration.FileImport;
using Kaesseli.Application.Integration.NextOpenTransaction;
using Kaesseli.Application.Integration.TransactionQuery;
using Kaesseli.Domain.Accounts;
using Kaesseli.Domain.Integration;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;

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
               async (IMediator mediator, IFormFile file, [FromForm] Guid accountId, [FromForm] Guid accountingPeriodId) =>
               {
                   var extension = System.IO.Path.GetExtension(file.FileName);
                   if (extension == ".zip")
                       return await UploadZippedFiles(file, accountId, accountingPeriodId, mediator);

                   await using var fileStream = file.OpenReadStream();
                   return await UploadFile(fileStream, extension, accountId, accountingPeriodId, mediator);
               })
           .Accepts<IFormFile>(contentType: "multipart/form-data")
           .DisableAntiforgery();
        return app;
    }

    private static async Task<Guid> UploadZippedFiles(IFormFile file, Guid accountId, Guid accountingPeriodId, IMediator mediator)
    {
        await using var memoryStream = file.OpenReadStream();
        using var archive = new ZipArchive(memoryStream);
        foreach (var entry in archive.Entries)
        {
            await using var entryStream = entry.Open();
            var extension = System.IO.Path.GetExtension(entry.FullName);
            var formFile = new FormFile(entryStream, 0, entry.Length, entry.Name, entry.FullName);
            await UploadFile(entryStream, extension, accountId, accountingPeriodId, mediator);
        }
        return Guid.Empty; // Return a default value or handle appropriately
    }

    private static async Task<Guid> UploadFile(Stream stream, string extension, Guid accountId, Guid accountingPeriodId, IMediator mediator)
    {
        var fileType = extension switch
        {
            ".csv" => FileType.PostFinanceCsv,
            ".camt" or ".xml" => FileType.Camt,
            _ => throw new ArgumentOutOfRangeException()
        };
        var command = new ProcessFileCommand
        {
            Content = stream,
            AccountId = accountId,
            FileType = fileType,
            AccountingPeriodId = accountingPeriodId
        };

        return await mediator.Send(command);
    }
}