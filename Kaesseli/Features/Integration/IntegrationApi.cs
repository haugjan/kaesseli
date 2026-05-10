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

            app.MapPatch(
                pattern: "/transaction/ignore",
                async (SetIgnoreTransaction.IHandler handler, [FromBody] SetIgnoreTransaction.Query cmd, CancellationToken ct) =>
                    await handler.Handle(cmd, ct)
            );

            app.MapPost(
                    pattern: "/file/upload",
                    async (
                        ProcessFile.IHandler handler,
                        ICamtProcessor camtProcessor,
                        IFormFile file,
                        [FromForm] Guid accountId,
                        [FromForm] Guid accountingPeriodId,
                        [FromQuery] bool? ignoreBalanceMismatch,
                        CancellationToken ct
                    ) =>
                    {
                        var force = ignoreBalanceMismatch.GetValueOrDefault();
                        var extension = System.IO.Path.GetExtension(file.FileName);
                        if (extension == ".zip")
                            return await UploadZippedFiles(file, accountId, accountingPeriodId, force, handler, camtProcessor, ct);

                        await using var fileStream = file.OpenReadStream();
                        return await UploadSingleFile(fileStream, file.FileName, extension, accountId, accountingPeriodId, force, handler, ct);
                    }
                )
                .Accepts<IFormFile>(contentType: "multipart/form-data")
                .DisableAntiforgery();
            return app;
        }
    }

    private static async Task<IResult> UploadZippedFiles(
        IFormFile file,
        Guid accountId,
        Guid accountingPeriodId,
        bool force,
        ProcessFile.IHandler handler,
        ICamtProcessor camtProcessor,
        CancellationToken ct)
    {
        await using var memoryStream = file.OpenReadStream();
        using var archive = new ZipArchive(memoryStream);

        var preparedFiles = new List<(string Name, byte[] Bytes, FileType Type)>();
        foreach (var entry in archive.Entries)
        {
            var extension = System.IO.Path.GetExtension(entry.FullName);
            var fileType = ResolveFileType(extension);
            await using var entryStream = entry.Open();
            using var memory = new MemoryStream();
            await entryStream.CopyToAsync(memory, ct);
            preparedFiles.Add((entry.FullName, memory.ToArray(), fileType));
        }

        if (!force)
        {
            var mismatches = new List<Contracts.Integration.BalanceMismatch>();
            foreach (var (name, bytes, type) in preparedFiles)
            {
                if (type != FileType.Camt) continue;
                using var parseStream = new MemoryStream(bytes);
                var doc = await camtProcessor.ReadCamtFile(parseStream, ct);
                if (!doc.IsBalanceConsistent)
                    mismatches.Add(BuildMismatch(name, doc));
            }

            if (mismatches.Count > 0)
                return Results.Json(
                    new Contracts.Integration.BalanceMismatchResponse(mismatches),
                    statusCode: 422);
        }

        Guid lastId = Guid.Empty;
        foreach (var (_, bytes, type) in preparedFiles)
        {
            using var importStream = new MemoryStream(bytes);
            lastId = await handler.Handle(
                new ProcessFile.Query(type, importStream, accountId, accountingPeriodId, IgnoreBalanceMismatch: true),
                ct);
        }
        return Results.Ok(lastId);
    }

    private static async Task<IResult> UploadSingleFile(
        Stream stream,
        string fileName,
        string extension,
        Guid accountId,
        Guid accountingPeriodId,
        bool force,
        ProcessFile.IHandler handler,
        CancellationToken ct)
    {
        var fileType = ResolveFileType(extension);
        var command = new ProcessFile.Query(fileType, stream, accountId, accountingPeriodId, force);

        try
        {
            return Results.Ok(await handler.Handle(command, ct));
        }
        catch (BalanceMismatchException ex)
        {
            return Results.Json(
                new Contracts.Integration.BalanceMismatchResponse([BuildMismatch(fileName, ex.Document)]),
                statusCode: 422);
        }
    }

    private static FileType ResolveFileType(string extension) => extension switch
    {
        ".csv" => FileType.PostFinanceCsv,
        ".camt" or ".xml" => FileType.Camt,
        _ => throw new ArgumentOutOfRangeException(nameof(extension), extension, $"Unsupported file extension '{extension}'"),
    };

    private static Contracts.Integration.BalanceMismatch BuildMismatch(string fileName, FinancialDocument document) =>
        new(
            FileName: fileName,
            ExpectedDelta: document.ExpectedDelta,
            ActualDelta: document.EntriesTotal,
            Difference: document.BalanceDifference,
            ValueDateFrom: document.ValueDateFrom,
            ValueDateTo: document.ValueDateTo);
}
