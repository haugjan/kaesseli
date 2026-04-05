using System.IO;
using System.IO.Compression;
using Kaesseli.Features.Integration.FileImport;
using Kaesseli.Features.Integration.NextOpenTransaction;
using Kaesseli.Features.Integration.TransactionQuery;
using Microsoft.AspNetCore.Mvc;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Routing;

public static class IntegrationApi
{
    extension(IEndpointRouteBuilder app)
    {
        // ReSharper disable once UnusedMethodReturnValue.Global
        public IEndpointRouteBuilder MapIntegrationEndpoints()
        {
            app.MapGet(
                pattern: "/transactionSummary",
                async (GetTransactionSummaries.IHandler handler, CancellationToken ct) =>
                    await handler.Handle(ct)
            );
            app.MapGet(
                pattern: "/transaction",
                async (GetTransactions.IHandler handler, [FromQuery] Guid transactionSummaryId, CancellationToken ct) =>
                    await handler.Handle(new GetTransactions.Query(transactionSummaryId), ct)
            );

            app.MapGet(
                pattern: "/transaction/nextOpen",
                async (GetNextOpenTransaction.IHandler handler, [FromQuery] int? skip, CancellationToken ct) =>
                    await handler.Handle(new GetNextOpenTransaction.Query(skip.GetValueOrDefault()), ct)
            );
            app.MapGet(
                pattern: "/transaction/totalOpen",
                async (GetTotalOpenTransaction.IHandler handler, CancellationToken ct) =>
                    await handler.Handle(ct)
            );

            app.MapPatch(
                pattern: "/transaction/journalEntry",
                async (AssignOpenTransaction.IHandler handler, [FromBody] AssignOpenTransaction.Query cmd, CancellationToken ct) =>
                    await handler.Handle(cmd, ct)
            );

            app.MapPatch(
                pattern: "/transaction/journalEntry/split",
                async (SplitOpenTransaction.IHandler handler, [FromBody] SplitOpenTransaction.Query cmd, CancellationToken ct) =>
                    await handler.Handle(cmd, ct)
            );

            app.MapPost(
                    pattern: "/file/upload",
                    async (
                        ProcessFile.IHandler handler,
                        IFormFile file,
                        [FromForm] Guid accountId,
                        [FromForm] Guid accountingPeriodId,
                        CancellationToken ct
                    ) =>
                    {
                        var extension = System.IO.Path.GetExtension(file.FileName);
                        if (extension == ".zip")
                            return await UploadZippedFiles(file, accountId, accountingPeriodId, handler, ct);

                        await using var fileStream = file.OpenReadStream();
                        return await UploadFile(fileStream, extension, accountId, accountingPeriodId, handler, ct);
                    }
                )
                .Accepts<IFormFile>(contentType: "multipart/form-data")
                .DisableAntiforgery();
            return app;
        }
    }

    private static async Task<Guid> UploadZippedFiles(
        IFormFile file, Guid accountId, Guid accountingPeriodId,
        ProcessFile.IHandler handler, CancellationToken ct)
    {
        await using var memoryStream = file.OpenReadStream();
        using var archive = new ZipArchive(memoryStream);
        foreach (var entry in archive.Entries)
        {
            await using var entryStream = entry.Open();
            var extension = System.IO.Path.GetExtension(entry.FullName);
            await UploadFile(entryStream, extension, accountId, accountingPeriodId, handler, ct);
        }
        return Guid.Empty;
    }

    private static async Task<Guid> UploadFile(
        Stream stream, string extension, Guid accountId, Guid accountingPeriodId,
        ProcessFile.IHandler handler, CancellationToken ct)
    {
        var fileType = extension switch
        {
            ".csv" => FileType.PostFinanceCsv,
            ".camt" or ".xml" => FileType.Camt,
            _ => throw new ArgumentOutOfRangeException(),
        };
        var command = new ProcessFile.Query(
            FileType: fileType, Content: stream,
            AccountId: accountId, AccountingPeriodId: accountingPeriodId);

        return await handler.Handle(command, ct);
    }
}
