using FluentAssertions;
using Kaesseli.Application.Integration.Camt;
using Kaesseli.Infrastructure.Integration;
using Xunit;

namespace Kaesseli.Infrastructure.Test.Integration;

public class CamtProcessorTests
{
    [Fact]
    public async Task ReadCamtFile_ReturnsCorrectData()
    {
        //Arrange
        var processor = new CamtProcessor();
        await using var content = GetType().ReadResourceAsStream(fileName: "ExampleData.Example.camt");
        var cancellationToken = new CancellationToken();

        //Act
        var current = await processor.ReadCamtFile(content, cancellationToken);
        // ReSharper disable StringLiteralTypo
        var expected = await CreateExpected2();

        //Assert
        current.Should().BeEquivalentTo(expected);
    }

    private async Task<CamtDocument> CreateExpected2() =>
        new()
        {
            CamtEntries = await CreateCamtEntries(),
            BalanceBefore = 11708.31m,
            BalanceAfter = 19513.79m,
            ValueDateFrom = new DateOnly(year: 2024, month: 1, day: 22),
            ValueDateTo = new DateOnly(year: 2024, month: 1, day: 31),
            Reference = "MSG20240201014110"
        };

    private async Task<List<CamtEntry>> CreateCamtEntries() =>
    [
        new()
        {
            Description = "Sample transaction details",
            Amount = -29.95m,
            ValueDate = new DateOnly(
                year: 2024,
                month: 01,
                day: 25),
            BookDate = new DateOnly(
                year: 2024,
                month: 01,
                day: 24),
            RawText = await GetType()
                          .ReadResource(fileName: "ExampleData.RawText1.yaml"),
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
            Creditor = "Fictional Company"
        },

        new()
        {
            Description = "Credit transaction details",
            Amount = 13656.85m,
            ValueDate = new DateOnly(
                year: 2024,
                month: 01,
                day: 27),
            BookDate = new DateOnly(
                year: 2024,
                month: 01,
                day: 26),
            RawText = await GetType()
                          .ReadResource(fileName: "ExampleData.RawText2.yaml"),
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
            Creditor = null
        }
    ];
}