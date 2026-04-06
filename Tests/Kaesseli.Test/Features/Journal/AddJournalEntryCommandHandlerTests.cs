using Kaesseli.Features.Journal;
using Kaesseli.Features.Accounts;
using Kaesseli.Test.Faker;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using Xunit;

namespace Kaesseli.Test.Features.Journal;

public class AddJournalEntryCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldAddAccountSuccessfully()
    {
        // Arrange
        var mockJournalRepo = Substitute.For<IJournalRepository>();
        var mockAccountRepo = Substitute.For<IAccountRepository>();
        var debitAccount = Account.Create("Debit", AccountType.Expense, new AccountIcon("favorite", "blue"));
        var creditAccount = Account.Create("Credit", AccountType.Revenue, new AccountIcon("favorite", "blue"));
        var accountingPeriod = AccountingPeriod.Create("Test Period", default, default);
        var command = new SmartFaker<AddJournalEntry.Query>()
            .RuleFor(c => c.DebitAccountId, debitAccount.Id)
            .RuleFor(c => c.CreditAccountId, creditAccount.Id)
            .RuleFor(c => c.AccountingPeriodId, accountingPeriod.Id)
            .Generate();
        var cancellationToken = new CancellationToken();

        mockJournalRepo
            .AddJournalEntry(
                Arg.Is<JournalEntry>(a =>
                    a.Amount == command.Amount && a.Description == command.Description
                ),
                cancellationToken
            )
            .Returns(callInfo => callInfo.ArgAt<JournalEntry>(0));
        mockAccountRepo
            .GetAccount(debitAccount.Id, Arg.Any<CancellationToken>())
            .Returns(debitAccount);
        mockAccountRepo
            .GetAccount(creditAccount.Id, Arg.Any<CancellationToken>())
            .Returns(creditAccount);
        mockAccountRepo
            .GetAccountingPeriod(accountingPeriod.Id, Arg.Any<CancellationToken>())
            .Returns(accountingPeriod);

        var handler = new AddJournalEntry.Handler(
            mockJournalRepo,
            mockAccountRepo,
            TimeProvider.System
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        await mockJournalRepo.Received()
            .AddJournalEntry(
                Arg.Is<JournalEntry>(entry =>
                    entry.Amount == command.Amount
                    && entry.Description == command.Description
                    && entry.ValueDate == command.ValueDate
                    && entry.Id == result
                ),
                cancellationToken
            );
    }

    [Fact]
    public async Task Handle_EmptyValueDate_ShouldAddAccountWithCurrentDate()
    {
        // Arrange
        var mockJournalRepo = Substitute.For<IJournalRepository>();
        var mockAccountRepo = Substitute.For<IAccountRepository>();
        var currentDay = new DateOnly(year: 1982, month: 11, day: 3);
        var fakeTimeProvider = new FakeTimeProvider(new DateTimeOffset(currentDay.ToDateTime(TimeOnly.MinValue)));
        var debitAccount = Account.Create("Debit", AccountType.Expense, new AccountIcon("favorite", "blue"));
        var creditAccount = Account.Create("Credit", AccountType.Revenue, new AccountIcon("favorite", "blue"));
        var accountingPeriod = AccountingPeriod.Create("Test Period", default, default);
        var command = new SmartFaker<AddJournalEntry.Query>()
            .RuleFor(c => c.ValueDate, _ => null)
            .RuleFor(c => c.DebitAccountId, debitAccount.Id)
            .RuleFor(c => c.CreditAccountId, creditAccount.Id)
            .RuleFor(c => c.AccountingPeriodId, accountingPeriod.Id)
            .Generate();
        var cancellationToken = new CancellationToken();

        mockJournalRepo
            .AddJournalEntry(
                Arg.Is<JournalEntry>(a =>
                    a.Amount == command.Amount && a.Description == command.Description
                ),
                cancellationToken
            )
            .Returns(callInfo => callInfo.ArgAt<JournalEntry>(0));
        mockAccountRepo
            .GetAccount(debitAccount.Id, Arg.Any<CancellationToken>())
            .Returns(debitAccount);
        mockAccountRepo
            .GetAccount(creditAccount.Id, Arg.Any<CancellationToken>())
            .Returns(creditAccount);
        mockAccountRepo
            .GetAccountingPeriod(accountingPeriod.Id, Arg.Any<CancellationToken>())
            .Returns(accountingPeriod);

        var handler = new AddJournalEntry.Handler(
            mockJournalRepo,
            mockAccountRepo,
            fakeTimeProvider
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        await mockJournalRepo.Received()
            .AddJournalEntry(
                Arg.Is<JournalEntry>(entry =>
                    entry.Amount == command.Amount
                    && entry.Description == command.Description
                    && entry.ValueDate == currentDay
                    && entry.Id == result
                ),
                cancellationToken
            );
    }
}
