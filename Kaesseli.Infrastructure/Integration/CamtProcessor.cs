using System.Xml.Serialization;
using Kaesseli.Application.Integration;

namespace Kaesseli.Infrastructure.Integration;

internal class CamtProcessor : ICamtProcessor
{
    public async Task<CamtDocument> ReadCamtFile(Stream content, CancellationToken cancellationToken)
    {
        var serializer = new XmlSerializer(type: typeof(Document));
        using var reader = new StreamReader(content);
        var document = await Task.Run(() => (Document)serializer.Deserialize(reader)!, cancellationToken)
                    ?? throw new FormatException(message: "Could not deserialize CAMT053");

        ThrowExceptionIfFailures(document);

        var firstStatement = document.BkToCstmrStmt.Stmt.Single();

        var camtDocument = new CamtDocument
        {
            CamtEntries = CreateCamtEntries(firstStatement),
            BalanceBefore = firstStatement.Bal.Single(balance => balance.Tp.CdOrPrtry.Item is BalanceType12Code.OPBD).Amt.Value,
            BalanceAfter = firstStatement.Bal.Single(balance => balance.Tp.CdOrPrtry.Item is BalanceType12Code.CLBD).Amt.Value,
            ValueDateFrom = DateOnly.FromDateTime(firstStatement.FrToDt.FrDtTm),
            ValueDateTo = DateOnly.FromDateTime(firstStatement.FrToDt.ToDtTm),
            Reference = document.BkToCstmrStmt.GrpHdr.MsgId
        };

        return camtDocument;
    }

    private static IEnumerable<CamtEntry> CreateCamtEntries(AccountStatement4 accountStatement) =>
        accountStatement.Ntry
                        .Select(
                            entry => new CamtEntry
                            {
                                RawText = entry.NtryDtls.ToYaml(),
                                Description = entry.AddtlNtryInf,
                                Reference = entry.AcctSvcrRef,
                                TransactionCodeDetail = entry.BkTxCd.ToYaml(),
                                TransactionCode = entry.BkTxCd.Domn.Cd,
                                Amount = entry.CdtDbtInd == CreditDebitCode.CRDT ? entry.Amt.Value : -entry.Amt.Value,
                                ValueDate = DateOnly.FromDateTime(entry.ValDt.Item),
                                BookDate = DateOnly.FromDateTime(entry.BookgDt.Item)
                            });

    private static void ThrowExceptionIfFailures(Document document)
    {
        if (document.BkToCstmrStmt.Stmt
                    .SelectMany(stmt => stmt.Ntry)
                    .Any(entry => entry.Sts != EntryStatus2Code.BOOK))
            throw new FormatException(message: "Found journal entry with status code not equals to 'book'");

        if (document.BkToCstmrStmt.Stmt.Length != 1)
            throw new FormatException(
                message: $"Only one account statement per document allowed, but found {document.BkToCstmrStmt.Stmt.Length}");
    }
}