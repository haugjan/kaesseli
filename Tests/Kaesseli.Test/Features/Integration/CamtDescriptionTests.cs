using System.Text;
using Kaesseli.Features.Integration.FileImport;
using Shouldly;
using Xunit;

namespace Kaesseli.Test.Features.Integration;

public class CamtDescriptionTests
{
    [Fact]
    public async Task CardTransaction_UsesPoiMerchantWithoutCardBoilerplate()
    {
        var processor = new CamtProcessor();
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(CardEntryXml));

        var document = await processor.ReadCamtFile(stream, CancellationToken.None);

        var entry = document.Entries.Single();
        entry.Description.ShouldBe("JUMBO-2646 Gruzemarkt 0840 Winterth");
    }

    [Fact]
    public async Task Transfer_UsesCreditorAndUstrdInsteadOfAddtlNtryInf()
    {
        var processor = new CamtProcessor();
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(TransferEntryXml));

        var document = await processor.ReadCamtFile(stream, CancellationToken.None);

        var entry = document.Entries.Single();
        entry.Description.ShouldBe("Müller AG – Rechnung 12345 vom Mai");
    }

    [Fact]
    public async Task FallbackToAddtlNtryInf_WhenNoPoiAndNoTxDtlsParty()
    {
        var processor = new CamtProcessor();
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(FallbackEntryXml));

        var document = await processor.ReadCamtFile(stream, CancellationToken.None);

        var entry = document.Entries.Single();
        entry.Description.ShouldBe("Generic statement note");
    }

    private const string CardEntryXml = """
        <?xml version="1.0" encoding="UTF-8"?>
        <Document xmlns="urn:iso:std:iso:20022:tech:xsd:camt.053.001.04">
          <BkToCstmrStmt>
            <GrpHdr><MsgId>MSG-CARD</MsgId><CreDtTm>2026-05-04T00:00:00</CreDtTm></GrpHdr>
            <Stmt>
              <Id>S1</Id>
              <CreDtTm>2026-05-04T00:00:00</CreDtTm>
              <FrToDt><FrDtTm>2026-05-04T00:00:00</FrDtTm><ToDtTm>2026-05-04T23:59:59</ToDtTm></FrToDt>
              <Acct><Id><IBAN>CH0000000000000000000</IBAN></Id></Acct>
              <Bal><Tp><CdOrPrtry><Cd>OPBD</Cd></CdOrPrtry></Tp><Amt Ccy="CHF">1000</Amt><CdtDbtInd>CRDT</CdtDbtInd><Dt><Dt>2026-05-04</Dt></Dt></Bal>
              <Bal><Tp><CdOrPrtry><Cd>CLBD</Cd></CdOrPrtry></Tp><Amt Ccy="CHF">924.95</Amt><CdtDbtInd>CRDT</CdtDbtInd><Dt><Dt>2026-05-04</Dt></Dt></Bal>
              <Ntry>
                <Amt Ccy="CHF">75.05</Amt><CdtDbtInd>DBIT</CdtDbtInd><RvslInd>false</RvslInd><Sts>BOOK</Sts>
                <BookgDt><Dt>2026-05-04</Dt></BookgDt><ValDt><Dt>2026-05-02</Dt></ValDt>
                <AcctSvcrRef>CARD-1</AcctSvcrRef>
                <BkTxCd><Domn><Cd>PMNT</Cd><Fmly><Cd>CCRD</Cd><SubFmlyCd>POSD</SubFmlyCd></Fmly></Domn></BkTxCd>
                <CardTx>
                  <Card><PlainCardData><PAN>00008393</PAN><CardSeqNb>000</CardSeqNb><XpryDt>2028-12</XpryDt></PlainCardData><CardBrnd><Id>50</Id></CardBrnd></Card>
                  <POI><Id><Id>JUMBO-2646 Gruzemarkt 0840 Winterth</Id></Id></POI>
                </CardTx>
                <AddtlNtryInf>Einkauf ZKB Visa Debit Card Nr. xxxx 8393, JUMBO-2646 Gruzemarkt 0840 Winterthur</AddtlNtryInf>
              </Ntry>
            </Stmt>
          </BkToCstmrStmt>
        </Document>
        """;

    private const string TransferEntryXml = """
        <?xml version="1.0" encoding="UTF-8"?>
        <Document xmlns="urn:iso:std:iso:20022:tech:xsd:camt.053.001.04">
          <BkToCstmrStmt>
            <GrpHdr><MsgId>MSG-XFER</MsgId><CreDtTm>2026-05-04T00:00:00</CreDtTm></GrpHdr>
            <Stmt>
              <Id>S1</Id>
              <CreDtTm>2026-05-04T00:00:00</CreDtTm>
              <FrToDt><FrDtTm>2026-05-04T00:00:00</FrDtTm><ToDtTm>2026-05-04T23:59:59</ToDtTm></FrToDt>
              <Acct><Id><IBAN>CH0000000000000000000</IBAN></Id></Acct>
              <Bal><Tp><CdOrPrtry><Cd>OPBD</Cd></CdOrPrtry></Tp><Amt Ccy="CHF">1000</Amt><CdtDbtInd>CRDT</CdtDbtInd><Dt><Dt>2026-05-04</Dt></Dt></Bal>
              <Bal><Tp><CdOrPrtry><Cd>CLBD</Cd></CdOrPrtry></Tp><Amt Ccy="CHF">800</Amt><CdtDbtInd>CRDT</CdtDbtInd><Dt><Dt>2026-05-04</Dt></Dt></Bal>
              <Ntry>
                <Amt Ccy="CHF">200</Amt><CdtDbtInd>DBIT</CdtDbtInd><RvslInd>false</RvslInd><Sts>BOOK</Sts>
                <BookgDt><Dt>2026-05-04</Dt></BookgDt><ValDt><Dt>2026-05-04</Dt></ValDt>
                <AcctSvcrRef>XFER-1</AcctSvcrRef>
                <BkTxCd><Domn><Cd>PMNT</Cd><Fmly><Cd>ICDT</Cd><SubFmlyCd>DMCT</SubFmlyCd></Fmly></Domn></BkTxCd>
                <NtryDtls>
                  <TxDtls>
                    <Refs><AcctSvcrRef>XFER-1-DTL</AcctSvcrRef></Refs>
                    <Amt Ccy="CHF">200</Amt><CdtDbtInd>DBIT</CdtDbtInd>
                    <RltdPties><Cdtr><Nm>Müller AG</Nm></Cdtr></RltdPties>
                    <RmtInf><Ustrd>Rechnung 12345 vom Mai</Ustrd></RmtInf>
                  </TxDtls>
                </NtryDtls>
                <AddtlNtryInf>SEPA-Überweisung an Müller AG</AddtlNtryInf>
              </Ntry>
            </Stmt>
          </BkToCstmrStmt>
        </Document>
        """;

    private const string FallbackEntryXml = """
        <?xml version="1.0" encoding="UTF-8"?>
        <Document xmlns="urn:iso:std:iso:20022:tech:xsd:camt.053.001.04">
          <BkToCstmrStmt>
            <GrpHdr><MsgId>MSG-FALLBACK</MsgId><CreDtTm>2026-05-04T00:00:00</CreDtTm></GrpHdr>
            <Stmt>
              <Id>S1</Id>
              <CreDtTm>2026-05-04T00:00:00</CreDtTm>
              <FrToDt><FrDtTm>2026-05-04T00:00:00</FrDtTm><ToDtTm>2026-05-04T23:59:59</ToDtTm></FrToDt>
              <Acct><Id><IBAN>CH0000000000000000000</IBAN></Id></Acct>
              <Bal><Tp><CdOrPrtry><Cd>OPBD</Cd></CdOrPrtry></Tp><Amt Ccy="CHF">1000</Amt><CdtDbtInd>CRDT</CdtDbtInd><Dt><Dt>2026-05-04</Dt></Dt></Bal>
              <Bal><Tp><CdOrPrtry><Cd>CLBD</Cd></CdOrPrtry></Tp><Amt Ccy="CHF">990</Amt><CdtDbtInd>CRDT</CdtDbtInd><Dt><Dt>2026-05-04</Dt></Dt></Bal>
              <Ntry>
                <Amt Ccy="CHF">10</Amt><CdtDbtInd>DBIT</CdtDbtInd><RvslInd>false</RvslInd><Sts>BOOK</Sts>
                <BookgDt><Dt>2026-05-04</Dt></BookgDt><ValDt><Dt>2026-05-04</Dt></ValDt>
                <AcctSvcrRef>FB-1</AcctSvcrRef>
                <BkTxCd><Domn><Cd>PMNT</Cd><Fmly><Cd>NTAV</Cd><SubFmlyCd>NTAV</SubFmlyCd></Fmly></Domn></BkTxCd>
                <AddtlNtryInf>Generic statement note</AddtlNtryInf>
              </Ntry>
            </Stmt>
          </BkToCstmrStmt>
        </Document>
        """;
}
