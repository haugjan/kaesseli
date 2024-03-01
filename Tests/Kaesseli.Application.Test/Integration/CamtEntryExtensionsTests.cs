using FluentAssertions;
using Kaesseli.Application.Integration.Camt;
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
        var camtEntry = new SmartFaker<CamtEntry>().Generate();

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
        var camtDocument = new SmartFaker<CamtDocument>()
                           .RuleFor(ce => ce.CamtEntries, value: new SmartFaker<CamtEntry>().Generate(count: 2))
                           .Generate();
        var account = new SmartFaker<Account>().Generate();

        //Act
        var transactionSummary = camtDocument.ToTransactionSummary(account);

        //Assert
        transactionSummary.Should()
                          .BeEquivalentTo(camtDocument, options => options.Excluding(cd => cd.CamtEntries));
        transactionSummary.Transactions.Should().BeEquivalentTo(camtDocument.CamtEntries);
    }
}