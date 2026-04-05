using Kaesseli.Features.Integration.FileImport;
using Kaesseli.Features.Accounts;
using Kaesseli.Test.Faker;
using Xunit;

namespace Kaesseli.Test.Application.Integration;

public class CamtEntryExtensionsTests
{
    [Fact]
    public void ToTransaction_TransformsCorrectly()
    {
        //Arange
        var camtEntry = new SmartFaker<FinancialDocumentEntry>().Generate();

        //Act
        var transaction = camtEntry.ToTransaction();

        //Assert
        Assert.Equivalent(camtEntry, transaction);
    }

    [Fact]
    public void ToTransactionSummary_TransformsCorrectly()
    {
        //Arange
        var document = new SmartFaker<FinancialDocument>()
            .RuleFor(
                ce => ce.Entries,
                value: new SmartFaker<FinancialDocumentEntry>().Generate(count: 2)
            )
            .Generate();
        var account = new SmartFaker<Account>().Generate();

        //Act
        var transactionSummary = document.ToTransactionSummary(account);

        //Assert
        Assert.Equivalent(document.Entries, transactionSummary.Transactions);
    }
}
