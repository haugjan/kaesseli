using System.Text;
using Kaesseli.Features.Integration.FileImport;
using Shouldly;
using Xunit;

namespace Kaesseli.Test.Features.Integration;

public class CamtBatchEntryTests
{
    [Fact]
    public async Task BatchEntry_WithMultipleTxDtls_IsSplitIntoIndividualEntries()
    {
        var processor = new CamtProcessor();
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(BatchXml));

        var document = await processor.ReadCamtFile(stream, CancellationToken.None);

        var entries = document.Entries.ToArray();
        entries.Length.ShouldBe(3);

        entries[0].Amount.ShouldBe(-480m);
        entries[0].Reference.ShouldBe("SUB-1");
        entries[0].Creditor.ShouldBe("Jasmine Haug");
        entries[0].Description.ShouldBe("Jasmine Haug – Haushalt 400.-, Taschengeld: 50.-");

        entries[1].Amount.ShouldBe(-79.5m);
        entries[1].Reference.ShouldBe("SUB-2");
        entries[1].Creditor.ShouldBe("Leonie Haug");
        entries[1].Description.ShouldBe("Leonie Haug – Leonie Sackgeld");

        entries[2].Amount.ShouldBe(-25m);
        entries[2].Reference.ShouldBe("SUB-3");
        entries[2].Creditor.ShouldBe("Enea Haug");
        entries[2].Description.ShouldBe("Enea Haug");

        // Sum of split entries equals the parent amount, so balance reconciliation still works.
        document.EntriesTotal.ShouldBe(-584.5m);
        document.ExpectedDelta.ShouldBe(-584.5m);
        document.IsBalanceConsistent.ShouldBeTrue();
    }

    [Fact]
    public async Task BalanceMismatch_IsDetectedWhenEntriesDoNotSumToBalanceDelta()
    {
        var processor = new CamtProcessor();
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(MismatchXml));

        var document = await processor.ReadCamtFile(stream, CancellationToken.None);

        document.HasBalanceInfo.ShouldBeTrue();
        document.IsBalanceConsistent.ShouldBeFalse();
        document.BalanceDifference.ShouldBe(50m);
    }

    private const string BatchXml = """
        <?xml version="1.0" encoding="UTF-8"?>
        <Document xmlns="urn:iso:std:iso:20022:tech:xsd:camt.053.001.04">
          <BkToCstmrStmt>
            <GrpHdr><MsgId>MSG-BATCH</MsgId><CreDtTm>2026-05-04T00:00:00</CreDtTm></GrpHdr>
            <Stmt>
              <Id>S1</Id>
              <CreDtTm>2026-05-04T00:00:00</CreDtTm>
              <FrToDt><FrDtTm>2026-05-04T00:00:00</FrDtTm><ToDtTm>2026-05-04T23:59:59</ToDtTm></FrToDt>
              <Acct><Id><IBAN>CH0000000000000000000</IBAN></Id></Acct>
              <Bal><Tp><CdOrPrtry><Cd>OPBD</Cd></CdOrPrtry></Tp><Amt Ccy="CHF">1000</Amt><CdtDbtInd>CRDT</CdtDbtInd><Dt><Dt>2026-05-04</Dt></Dt></Bal>
              <Bal><Tp><CdOrPrtry><Cd>CLBD</Cd></CdOrPrtry></Tp><Amt Ccy="CHF">415.5</Amt><CdtDbtInd>CRDT</CdtDbtInd><Dt><Dt>2026-05-04</Dt></Dt></Bal>
              <Ntry>
                <Amt Ccy="CHF">584.5</Amt><CdtDbtInd>DBIT</CdtDbtInd><RvslInd>false</RvslInd><Sts>BOOK</Sts>
                <BookgDt><Dt>2026-05-04</Dt></BookgDt><ValDt><Dt>2026-05-04</Dt></ValDt>
                <AcctSvcrRef>BATCH-PARENT</AcctSvcrRef>
                <BkTxCd><Domn><Cd>PMNT</Cd><Fmly><Cd>ICDT</Cd><SubFmlyCd>STDO</SubFmlyCd></Fmly></Domn></BkTxCd>
                <NtryDtls>
                  <Btch><NbOfTxs>3</NbOfTxs><TtlAmt Ccy="CHF">584.5</TtlAmt></Btch>
                  <TxDtls>
                    <Refs><AcctSvcrRef>SUB-1</AcctSvcrRef></Refs>
                    <Amt Ccy="CHF">480</Amt><CdtDbtInd>DBIT</CdtDbtInd>
                    <RltdPties><Cdtr><Nm>Jasmine Haug</Nm></Cdtr></RltdPties>
                    <RmtInf><Ustrd>Haushalt 400.-, Taschengeld: 50.-</Ustrd></RmtInf>
                  </TxDtls>
                  <TxDtls>
                    <Refs><AcctSvcrRef>SUB-2</AcctSvcrRef></Refs>
                    <Amt Ccy="CHF">79.5</Amt><CdtDbtInd>DBIT</CdtDbtInd>
                    <RltdPties><Cdtr><Nm>Leonie Haug</Nm></Cdtr></RltdPties>
                    <RmtInf><Ustrd>Leonie Sackgeld</Ustrd></RmtInf>
                  </TxDtls>
                  <TxDtls>
                    <Refs><AcctSvcrRef>SUB-3</AcctSvcrRef></Refs>
                    <Amt Ccy="CHF">25</Amt><CdtDbtInd>DBIT</CdtDbtInd>
                    <RltdPties><Cdtr><Nm>Enea Haug</Nm></Cdtr></RltdPties>
                  </TxDtls>
                </NtryDtls>
                <AddtlNtryInf>Belastungen Dauerauftrag (3): Auftrags-Nr. Z261244587890</AddtlNtryInf>
              </Ntry>
            </Stmt>
          </BkToCstmrStmt>
        </Document>
        """;

    private const string MismatchXml = """
        <?xml version="1.0" encoding="UTF-8"?>
        <Document xmlns="urn:iso:std:iso:20022:tech:xsd:camt.053.001.04">
          <BkToCstmrStmt>
            <GrpHdr><MsgId>MSG-MISMATCH</MsgId><CreDtTm>2026-05-04T00:00:00</CreDtTm></GrpHdr>
            <Stmt>
              <Id>S1</Id>
              <CreDtTm>2026-05-04T00:00:00</CreDtTm>
              <FrToDt><FrDtTm>2026-05-04T00:00:00</FrDtTm><ToDtTm>2026-05-04T23:59:59</ToDtTm></FrToDt>
              <Acct><Id><IBAN>CH0000000000000000000</IBAN></Id></Acct>
              <Bal><Tp><CdOrPrtry><Cd>OPBD</Cd></CdOrPrtry></Tp><Amt Ccy="CHF">1000</Amt><CdtDbtInd>CRDT</CdtDbtInd><Dt><Dt>2026-05-04</Dt></Dt></Bal>
              <Bal><Tp><CdOrPrtry><Cd>CLBD</Cd></CdOrPrtry></Tp><Amt Ccy="CHF">900</Amt><CdtDbtInd>CRDT</CdtDbtInd><Dt><Dt>2026-05-04</Dt></Dt></Bal>
              <Ntry>
                <Amt Ccy="CHF">50</Amt><CdtDbtInd>DBIT</CdtDbtInd><RvslInd>false</RvslInd><Sts>BOOK</Sts>
                <BookgDt><Dt>2026-05-04</Dt></BookgDt><ValDt><Dt>2026-05-04</Dt></ValDt>
                <AcctSvcrRef>SINGLE-1</AcctSvcrRef>
                <BkTxCd><Domn><Cd>PMNT</Cd><Fmly><Cd>CCRD</Cd><SubFmlyCd>POSD</SubFmlyCd></Fmly></Domn></BkTxCd>
                <AddtlNtryInf>Single entry that does not cover the closing balance delta</AddtlNtryInf>
              </Ntry>
            </Stmt>
          </BkToCstmrStmt>
        </Document>
        """;
}
