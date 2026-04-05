using Kaesseli.Features.Integration.FileImport;
using Shouldly;
using Xunit;

namespace Kaesseli.Test.Features.Integration;

public class CamtProcessorTests
{
    private static string NormalizeYaml(string? yaml) =>
        string.Join("\n", (yaml ?? "").Replace("\r\n", "\n").Split('\n').Select(l => l.TrimEnd())) + "\n";

    [Fact]
    public async Task ReadCamtFile_ReturnsCorrectData()
    {
        //Arrange
        var processor = new CamtProcessor();
        await using var content = GetType()
            .ReadResourceAsStream(fileName: "ExampleData.Example.camt");
        var cancellationToken = new CancellationToken();

        //Act
        var current = await processor.ReadCamtFile(content, cancellationToken);
        // ReSharper disable StringLiteralTypo
        var expected = await CreateExpected2();

        //Assert
        current.BalanceBefore.ShouldBe(expected.BalanceBefore);
        current.BalanceAfter.ShouldBe(expected.BalanceAfter);
        current.ValueDateFrom.ShouldBe(expected.ValueDateFrom);
        current.ValueDateTo.ShouldBe(expected.ValueDateTo);
        current.Reference.ShouldBe(expected.Reference);

        var currentEntries = current.Entries.ToArray();
        var expectedEntries = (await CreateCamtEntries()).ToArray();
        currentEntries.Length.ShouldBe(expectedEntries.Length);

        for (var i = 0; i < expectedEntries.Length; i++)
        {
            var e = expectedEntries[i];
            var a = currentEntries[i];
            a.Description.ShouldBe(e.Description);
            a.Amount.ShouldBe(e.Amount);
            a.ValueDate.ShouldBe(e.ValueDate);
            a.BookDate.ShouldBe(e.BookDate);
            a.Reference.ShouldBe(e.Reference);
            a.TransactionCode.ShouldBe(e.TransactionCode);
            a.Debtor.ShouldBe(e.Debtor);
            a.Creditor.ShouldBe(e.Creditor);
            NormalizeYaml(a.RawText).ShouldBe(NormalizeYaml(e.RawText));
            NormalizeYaml(a.TransactionCodeDetail).ShouldBe(NormalizeYaml(e.TransactionCodeDetail));
        }
    }

    private async Task<FinancialDocument> CreateExpected2() =>
        new()
        {
            Entries = await CreateCamtEntries(),
            BalanceBefore = 11708.31m,
            BalanceAfter = 19513.79m,
            ValueDateFrom = new DateOnly(year: 2024, month: 1, day: 22),
            ValueDateTo = new DateOnly(year: 2024, month: 1, day: 31),
            Reference = "MSG20240201014110",
        };

    private async Task<List<FinancialDocumentEntry>> CreateCamtEntries() =>
        [
            new()
            {
                Description = "Sample transaction details",
                Amount = -29.95m,
                ValueDate = new DateOnly(year: 2024, month: 01, day: 25),
                BookDate = new DateOnly(year: 2024, month: 01, day: 24),
                RawText = await GetType().ReadResource(fileName: "ExampleData.RawText1.yaml"),
                Reference = "REF00000000000001",
                TransactionCode = "PMNT",
                TransactionCodeDetail = """
                    Domn:
                      Cd: PMNT
                      Fmly:
                        Cd: ICDT
                        SubFmlyCd: BOOK
                    Prtry:

                    """,
                Debtor = null,
                Creditor = "Fictional Company",
            },
            new()
            {
                Description = "Credit transaction details",
                Amount = 13656.85m,
                ValueDate = new DateOnly(year: 2024, month: 01, day: 27),
                BookDate = new DateOnly(year: 2024, month: 01, day: 26),
                RawText = await GetType().ReadResource(fileName: "ExampleData.RawText2.yaml"),
                Reference = "REF00000000000002",
                TransactionCode = "PMNT",
                TransactionCodeDetail = """
                    Domn:
                      Cd: PMNT
                      Fmly:
                        Cd: RCDT
                        SubFmlyCd: AUTT
                    Prtry:

                    """,
                Debtor = "Another Fictional Company",
                Creditor = null,
            },
        ];
}
