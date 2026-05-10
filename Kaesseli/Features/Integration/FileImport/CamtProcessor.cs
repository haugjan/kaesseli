using System.Xml.Serialization;

namespace Kaesseli.Features.Integration.FileImport;

internal class CamtProcessor : ICamtProcessor
{
    public async Task<FinancialDocument> ReadCamtFile(Stream content, CancellationToken cancellationToken)
    {
        var serializer = new XmlSerializer(type: typeof(Document));
        using var reader = new StreamReader(content);
        var document = await Task.Run(() => (Document)serializer.Deserialize(reader)!, cancellationToken)
                    ?? throw new FormatException(message: "Could not deserialize CAMT053");

        ThrowExceptionIfFailures(document);

        var firstStatement = document.BkToCstmrStmt.Stmt.Single();

        var openingBalance = firstStatement.Bal.Single(b => b.Tp.CdOrPrtry.Item is BalanceType12Code.OPBD);
        var closingBalance = firstStatement.Bal.Single(b => b.Tp.CdOrPrtry.Item is BalanceType12Code.CLBD);

        return new FinancialDocument
        {
            Entries = CreateCamtEntries(firstStatement),
            BalanceBefore = SignedAmount(openingBalance.Amt.Value, openingBalance.CdtDbtInd),
            BalanceAfter = SignedAmount(closingBalance.Amt.Value, closingBalance.CdtDbtInd),
            ValueDateFrom = DateOnly.FromDateTime(firstStatement.FrToDt.FrDtTm),
            ValueDateTo = DateOnly.FromDateTime(firstStatement.FrToDt.ToDtTm),
            Reference = document.BkToCstmrStmt.GrpHdr.MsgId,
            HasBalanceInfo = true
        };
    }

    private static IEnumerable<FinancialDocumentEntry> CreateCamtEntries(AccountStatement4 accountStatement)
    {
        foreach (var entry in accountStatement.Ntry)
        {
            var subEntries = entry.NtryDtls?.SelectMany(d => d.TxDtls ?? []).ToArray() ?? [];
            if (subEntries.Length > 1)
            {
                for (var i = 0; i < subEntries.Length; i++)
                    yield return CreateBatchPart(entry, subEntries[i], i);
            }
            else
            {
                yield return CreateSingleEntry(entry);
            }
        }
    }

    private static FinancialDocumentEntry CreateSingleEntry(ReportEntry4 entry) =>
        new()
        {
            RawText = entry.NtryDtls.ToYaml(),
            Description = BuildSingleDescription(entry),
            Reference = entry.AcctSvcrRef,
            TransactionCodeDetail = entry.BkTxCd.ToYaml(),
            TransactionCode = entry.BkTxCd.Domn.Cd,
            Amount = SignedAmount(entry.Amt.Value, entry.CdtDbtInd),
            ValueDate = DateOnly.FromDateTime(entry.ValDt.Item),
            BookDate = DateOnly.FromDateTime(entry.BookgDt.Item),
            Debtor = GetSingleDebtor(entry),
            Creditor = GetSingleCreditor(entry)
        };

    private static string BuildSingleDescription(ReportEntry4 entry)
    {
        // Card transactions: AddtlNtryInf wraps the merchant name in boilerplate
        // ("Einkauf ZKB Visa Debit Card Nr. xxxx 8393, JUMBO …"). The merchant
        // alone is in CardTx/POI/Id/Id and is far more useful for matching.
        var poi = entry.CardTx?.POI?.Id?.Id;
        if (!string.IsNullOrWhiteSpace(poi))
            return poi.Trim();

        // Non-batch transfer with a TxDtls: prefer creditor/debtor + remittance
        // info — they describe the actual payment instead of the bank's wrapper.
        var txDtls = entry.NtryDtls?
                          .SelectMany(d => d.TxDtls ?? [])
                          .ToArray() ?? [];
        if (txDtls.Length == 1)
        {
            var combined = CombinePartyAndRemittance(txDtls[0]);
            if (!string.IsNullOrWhiteSpace(combined))
                return combined;
        }

        return entry.AddtlNtryInf ?? string.Empty;
    }

    private static string? CombinePartyAndRemittance(EntryTransaction4 tx)
    {
        var party = tx.RltdPties?.Cdtr?.Nm ?? tx.RltdPties?.Dbtr?.Nm;
        var remit = tx.RmtInf?.Ustrd is { Length: > 0 } lines
                        ? string.Join(" / ", lines)
                        : null;

        if (!string.IsNullOrWhiteSpace(party) && !string.IsNullOrWhiteSpace(remit))
            return $"{party} – {remit}";
        if (!string.IsNullOrWhiteSpace(party))
            return party;
        if (!string.IsNullOrWhiteSpace(remit))
            return remit;
        return null;
    }

    private static FinancialDocumentEntry CreateBatchPart(ReportEntry4 parent, EntryTransaction4 part, int index) =>
        new()
        {
            RawText = part.ToYaml(),
            Description = BuildBatchDescription(parent, part),
            Reference = part.Refs?.AcctSvcrRef
                        ?? $"{parent.AcctSvcrRef}-{index + 1}",
            TransactionCodeDetail = (part.BkTxCd ?? parent.BkTxCd).ToYaml(),
            TransactionCode = (part.BkTxCd ?? parent.BkTxCd).Domn.Cd,
            Amount = SignedAmount(part.Amt.Value, part.CdtDbtInd),
            ValueDate = DateOnly.FromDateTime(parent.ValDt.Item),
            BookDate = DateOnly.FromDateTime(parent.BookgDt.Item),
            Debtor = part.RltdPties?.Dbtr?.Nm,
            Creditor = part.RltdPties?.Cdtr?.Nm
        };

    private static string BuildBatchDescription(ReportEntry4 parent, EntryTransaction4 part) =>
        CombinePartyAndRemittance(part) ?? parent.AddtlNtryInf;

    private static decimal SignedAmount(decimal amount, CreditDebitCode indicator) =>
        indicator == CreditDebitCode.CRDT ? amount : -amount;

    private static string? GetSingleDebtor(ReportEntry4 entry) =>
        entry.NtryDtls?
             .FirstOrDefault()?.TxDtls
             .FirstOrDefault()?
             .RltdPties?.Dbtr?.Nm;
    private static string? GetSingleCreditor(ReportEntry4 entry) =>
        entry.NtryDtls?
             .FirstOrDefault()?.TxDtls
             .FirstOrDefault()?
             .RltdPties?.Cdtr?.Nm;

    private static void ThrowExceptionIfFailures(Document document)
    {
        if (document.BkToCstmrStmt.Stmt
                    .SelectMany(stmt => stmt.Ntry)
                    .Any(entry => entry.Sts != EntryStatus2Code.BOOK))
            throw new FormatException(message: "Found journal entry with status code not equals to 'book'");

        if (document.BkToCstmrStmt.Stmt.Length != 1)
        {
            throw new FormatException(
                message: $"Only one account statement per document allowed, but found {document.BkToCstmrStmt.Stmt.Length}");
        }
    }
}
