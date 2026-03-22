using System.IO;
using System.IO.Compression;
using Kaesseli.Application.Integration.FileImport;
using Kaesseli.Application.Integration.NextOpenTransaction;
using Kaesseli.Application.Integration.TransactionQuery;
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
            async (IGetTransactionSummariesQueryHandler handler) =>
                await handler.Handle(new GetTransactionSummariesQuery(), default)
        );
        app.MapGet(
            pattern: "/transaction",
            async (IGetTransactionsQueryHandler handler, [FromQuery] Guid transactionSummaryId) =>
                await handler.Handle(new GetTransactionsQuery { TransactionSummaryId = transactionSummaryId }, default)
        );

        app.MapGet(
            pattern: "/transaction/nextOpen",
            async (IGetNextOpenTransactionQueryHandler handler, [FromQuery] int? skip) =>
                await handler.Handle(new GetNextOpenTransactionQuery { Skip = skip.GetValueOrDefault() }, default)
        );
        app.MapGet(
            pattern: "/transaction/totalOpen",
            async (IGetTotalOpenTransactionQueryHandler handler, [FromQuery] int? skip) =>
                await handler.Handle(new GetTotalOpenTransactionQuery(), default)
        );

        app.MapPatch(
            pattern: "/transaction/journalEntry",
            async (IAssignOpenTransactionCommandHandler handler, [FromBody] AssignOpenTransactionCommand cmd) =>
                await handler.Handle(cmd, default)
        );

        app.MapPatch(
            pattern: "/transaction/journalEntry/split",
            async (ISplitOpenTransactionCommandHandler handler, [FromBody] SplitOpenTransactionCommand cmd) =>
                await handler.Handle(cmd, default)
        );

        app.MapPost(
                pattern: "/file/upload",
                async (
                    IProcessFileCommandHandler handler,
                    IFormFile file,
                    [FromForm] Guid accountId,
                    [FromForm] Guid accountingPeriodId
                ) =>
                {
                    var extension = System.IO.Path.GetExtension(file.FileName);
                    if (extension == ".zip")
                        return await UploadZippedFiles(
                            file,
                            accountId,
                            accountingPeriodId,
                            handler
                        );

                    await using var fileStream = file.OpenReadStream();
                    return await UploadFile(
                        fileStream,
                        extension,
                        accountId,
                        accountingPeriodId,
                        handler
                    );
                }
            )
            .Accepts<IFormFile>(contentType: "multipart/form-data")
            .DisableAntiforgery();
        return app;
    }

    private static async Task<Guid> UploadZippedFiles(
        IFormFile file,
        Guid accountId,
        Guid accountingPeriodId,
        IProcessFileCommandHandler handler
    )
    {
        await using var memoryStream = file.OpenReadStream();
        using var archive = new ZipArchive(memoryStream);
        foreach (var entry in archive.Entries)
        {
            await using var entryStream = entry.Open();
            var extension = System.IO.Path.GetExtension(entry.FullName);
            var formFile = new FormFile(entryStream, 0, entry.Length, entry.Name, entry.FullName);
            await UploadFile(entryStream, extension, accountId, accountingPeriodId, handler);
        }
        return Guid.Empty; // Return a default value or handle appropriately
    }

    private static async Task<Guid> UploadFile(
        Stream stream,
        string extension,
        Guid accountId,
        Guid accountingPeriodId,
        IProcessFileCommandHandler handler
    )
    {
        var fileType = extension switch
        {
            ".csv" => FileType.PostFinanceCsv,
            ".camt" or ".xml" => FileType.Camt,
            _ => throw new ArgumentOutOfRangeException(),
        };
        var command = new ProcessFileCommand
        {
            Content = stream,
            AccountId = accountId,
            FileType = fileType,
            AccountingPeriodId = accountingPeriodId,
        };

        return await handler.Handle(command, default);
    }
}
