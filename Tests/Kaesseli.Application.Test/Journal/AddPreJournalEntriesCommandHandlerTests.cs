using System.Collections.Immutable;
using FluentAssertions;
using Kaesseli.Application.Journal;
using Kaesseli.Domain.Accounts;
using Kaesseli.Domain.Journal;
using Kaesseli.TestUtilities.Faker;
using Moq;
using Xunit;

namespace Kaesseli.Application.Test.Journal;

public class AddPreJournalEntriesCommandHandlerTests
{
    private readonly Mock<IJournalRepository> _journalRepo;
    private readonly Mock<IAccountRepository> _accountRepo;

    public AddPreJournalEntriesCommandHandlerTests()
    {
        _journalRepo = new Mock<IJournalRepository>();
        _accountRepo = new Mock<IAccountRepository>();
    }

    //[Fact]
    //public async Task Handle_ShouldAddPreJournalEntrySuccessfully()
    //{
    //    //Arrange
    //    var handler = new AddPreJournalEntriesCommandHandler(_journalRepo.Object, _accountRepo.Object);
    //    var values = new SmartFaker<AddPreJournalEntriesCommandEntry>()
    //                 .Generate(count: 2)
    //                 .ToImmutableList();
    //    var request = new AddPreJournalEntriesCommand { Entries = values };
    //    var cancellation = new CancellationToken();
    //    var entriesToVerify = new List<PreJournalEntry>();
    //    _journalRepo.Setup(
    //                    repository =>
    //                        repository.AddPreJournalEntry(It.IsAny<PreJournalEntry>(), cancellation))
    //                .Returns(
    //                    (PreJournalEntry entry, CancellationToken ct) =>
    //                    {
    //                        entriesToVerify.Add(entry);
    //                        return Task.FromResult(entry);
    //                    });

    //    //Act
    //    await handler.Handle(request, cancellation);

    //    //Assert
    //    _journalRepo
    //        .Verify(
    //            repo => repo
    //                .AddPreJournalEntry(
    //                    It.IsAny<PreJournalEntry>(),
    //                    cancellation), times: Times.Exactly(callCount: 2));
    //    entriesToVerify.Should().BeEquivalentTo(values, options => options.Excluding(x=>x.AccountId));

    //}
}