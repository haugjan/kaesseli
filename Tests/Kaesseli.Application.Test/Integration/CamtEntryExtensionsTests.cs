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
    public void ToPreJournalEntry_TransformsCorrectly()
    {
        //Arange
        var camtEntry = new SmartFaker<CamtEntry>().Generate();
        var account = new SmartFaker<Account>().Generate();

        //Act
        var preJournalEntry = camtEntry.ToPreJournalEntry(account);

        //Assert
        preJournalEntry.Should()
                       .BeEquivalentTo(camtEntry, options 
                                           => options.Excluding(entry => entry.AccountId));
        preJournalEntry.Account.Should().Be(account);
    }
}