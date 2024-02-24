using FluentAssertions;
using Kaesseli.Application.Integration;
using Kaesseli.Domain.Accounts;
using Kaesseli.Domain.Journal;
using Kaesseli.TestUtilities.Faker;
using Xunit;

namespace Kaesseli.Application.Test.Integration;

public class CamtEntryExtensionsTests
{
    [Fact]
    public void ToPaymentEntry_TransformsCorrectly()
    {
        //Arange
        var camtEntry = new SmartFaker<CamtEntry>().Generate();

        //Act
        var paymentEntry = camtEntry.ToPaymentEntry();

        //Assert
        paymentEntry.Should()
                       .BeEquivalentTo(camtEntry);
    }

    [Fact]
    public void ToAccountStatement_TransformsCorrectly()
    {
        //Arange
        var camtDocument = new SmartFaker<CamtDocument>().RuleFor(ce=> ce.CamtEntries,  value: new SmartFaker<CamtEntry>().Generate(count: 2) ).Generate();
        var account = new SmartFaker<Account>().Generate();

        //Act
        var accountStatement = camtDocument.ToAccountStatement(account);

        //Assert
        accountStatement.Should()
                    .BeEquivalentTo(camtDocument, options => options.Excluding(cd=> cd.CamtEntries));
        accountStatement.PaymentEntries.Should().BeEquivalentTo(camtDocument.CamtEntries);
    }
}