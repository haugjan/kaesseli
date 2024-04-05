using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Kaesseli.Application.Integration.FileImport;

namespace Kaesseli.Infrastructure.Integration;

internal class PostFinanceCsvProcessor : IPostFinanceCsvProcessor
{
    public Task<FinancialDocument> ReadCsvFile(Stream content, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(content);
        var config = new CsvConfiguration(CultureInfo.GetCultureInfo("de-CH"))
        {
            Delimiter = ";",
            HasHeaderRecord = true,
            ShouldSkipRecord = record =>
            {
                return record.Row.ColumnCount != 7;
            }
        };
        using var csv = new CsvReader(reader, config);
        var records = csv.GetRecords<PostFinanceCsvSchema>();

        var entries = records.Select(
                                 record => new FinancialDocumentEntry
                                 {
                                     Description = record.Avisierungstext,
                                     RawText = record.ToYaml(),
                                     Amount =
                                         record.GutschriftInChf.GetValueOrDefault()
                                       + record.LastschriftInChf.GetValueOrDefault(),
                                     ValueDate = DateOnly.FromDateTime(record.Datum),
                                     BookDate = DateOnly.FromDateTime(record.Datum),
                                     Reference = record.ToYaml().ToHash(),
                                     TransactionCode = record.Bewegungstyp,
                                     TransactionCodeDetail = string.Empty,
                                     Debtor = string.Empty,
                                     Creditor = string.Empty
                                 })
                             .ToList();

        var result = new FinancialDocument
        {
            Entries = entries,
            BalanceBefore = 0,
            BalanceAfter = 0,
            ValueDateFrom = entries.Min(entry => entry.ValueDate),
            ValueDateTo = entries.Max(entry => entry.ValueDate),
            Reference = entries.ToYaml().ToHash()
        };
        return Task.FromResult(result);
    }
}