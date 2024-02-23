using FluentAssertions;
using Kaesseli.Application.Integration;
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
        var content = await GetType().ReadResource(fileName: "Example.camt");
        var accountId = Guid.NewGuid();
        var cancellationToken = new CancellationToken();

        //Act
        var current = await processor.ReadCamtFile(content, accountId, cancellationToken);
                // ReSharper disable StringLiteralTypo
        var expected = await CreateExpected(accountId);

        //Assert
        current.Should().BeEquivalentTo(expected);
    }

    private async Task<List<CamtEntry>> CreateExpected(Guid accountId) =>
    [
        new()
        {
            Description = "Sample transaction details",
            AccountId = accountId,
            Amount = -29.95m,
            ValueDate = new DateOnly(
                year: 2024,
                month: 01,
                day: 25),
            BookDate = new DateOnly(
                year: 2024,
                month: 01,
                day: 24),
            RawText = await GetType().ReadResource(fileName: "RawText1.yaml"),
            Reference = "REF00000000000001",
            TransactionCode = "PMNT",
            TransactionCodeDetail = """
                                    Domn:
                                      Cd: PMNT
                                      Fmly:
                                        Cd: ICDT
                                        SubFmlyCd: BOOK
                                    Prtry: 
                                    
                                    """
        },

        new()
        {
            Description = "Credit transaction details",
            AccountId = accountId,
            Amount = 13656.85m,
            ValueDate = new DateOnly(
                year: 2024,
                month: 01,
                day: 27),
            BookDate = new DateOnly(
                year: 2024,
                month: 01,
                day: 26),
            RawText = await GetType().ReadResource(fileName: "RawText2.yaml"),
            Reference = "REF00000000000002",
            TransactionCode = "PMNT",
            TransactionCodeDetail = """
                                    Domn:
                                      Cd: PMNT
                                      Fmly:
                                        Cd: RCDT
                                        SubFmlyCd: AUTT
                                    Prtry: 
                                    
                                    """
        }
    ];
}