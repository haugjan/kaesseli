using FluentAssertions;
using Kaesseli.Application.Integration.FileImport;
using Kaesseli.Domain.Accounts;
using Kaesseli.Domain.Journal;
using Kaesseli.TestUtilities.Faker;
using Xunit;

namespace Kaesseli.Application.Test.Integration;

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
        transaction.Should()
                   .BeEquivalentTo(camtEntry);
    }

    [Fact]
    public void ToTransactionSummary_TransformsCorrectly()
    {
        //Arange
        var document = new SmartFaker<FinancialDocument>()
                           .RuleFor(ce => ce.Entries, value: new SmartFaker<FinancialDocumentEntry>().Generate(count: 2))
                           .Generate();
        var account = new SmartFaker<Account>().Generate();

        //Act
        var transactionSummary = document.ToTransactionSummary(account);

        //Assert
        transactionSummary.Should()
                          .BeEquivalentTo(document, options => options.Excluding(cd => cd.Entries));
        transactionSummary.Transactions.Should().BeEquivalentTo(document.Entries);
    }
}
