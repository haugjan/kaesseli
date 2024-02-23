using System.Xml.Serialization;
using Kaesseli.Application.Integration;

namespace Kaesseli.Infrastructure.Integration;

internal class CamtProcessor : ICamtProcessor
{
    public Task<IEnumerable<CamtEntry>> ReadCamtFile(string content, Guid accountId, CancellationToken cancellationToken)
    {
        var serializer = new XmlSerializer(type: typeof(Document));
        using var reader = new StringReader(content);
        var document = (Document)serializer.Deserialize(reader)!;
        if (document is null) throw new FormatException(message: "Could not deserialize CAMT053");

        ThrowExceptionIfFailures(document);

        var result = document.BkToCstmrStmt.Stmt.SelectMany(stmt => stmt.Ntry)
                             .Select(
                                 entry => new CamtEntry
                                 {
                                     RawText = entry.NtryDtls.ToYaml(),
                                     Description = entry.AddtlNtryInf,
                                     Reference = entry.AcctSvcrRef,
                                     TransactionCodeDetail=entry.BkTxCd.ToYaml(),
                                     TransactionCode=entry.BkTxCd.Domn.Cd,
                                     AccountId = accountId,
                                     Amount = entry.CdtDbtInd == CreditDebitCode.CRDT ? entry.Amt.Value : -entry.Amt.Value,
                                     ValueDate = DateOnly.FromDateTime(entry.ValDt.Item),
                                     BookDate = DateOnly.FromDateTime(entry.BookgDt.Item)
                                 });
        return Task.FromResult(result);
    }

    private static void ThrowExceptionIfFailures(Document document)
    {
    
        if (document.BkToCstmrStmt.Stmt
                    .SelectMany(stmt => stmt.Ntry)
                    .Any(entry => entry.Sts != EntryStatus2Code.BOOK))
        {
            throw new FormatException(message: "Found journal entry with status code not equals to 'book'");
        }
    }
}