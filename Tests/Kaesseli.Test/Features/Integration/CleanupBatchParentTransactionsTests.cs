using Kaesseli.Features.Accounts;
using Kaesseli.Features.Integration;
using Kaesseli.Features.Journal;
using Kaesseli.Infrastructure;
using Kaesseli.Test.Faker;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace Kaesseli.Test.Features.Integration;

public class CleanupBatchParentTransactionsTests
{
    private static KaesseliContext CreateContext(DbContextOptions<KaesseliContext> options) =>
        new(options, TimeProvider.System);

    private static Transaction CreateTransaction(string reference) =>
        new SmartFaker<Transaction>()
            .RuleFor(t => t.Reference, value: reference)
            .RuleFor(t => t.IsIgnored, value: false)
            .RuleFor(t => t.JournalEntries, value: null)
            .Generate();

    [Fact]
    public async Task CleanupBatchParentTransactions_DryRun_ReportsParentsWithoutDeleting()
    {
        var options = new DbContextOptionsBuilder<KaesseliContext>()
            .UseInMemoryDatabase(databaseName: "CleanupBatchParents_DryRun")
            .Options;

        var parent = CreateTransaction(reference: "ABC-PARENT");
        var sub1 = CreateTransaction(reference: "ABC-PARENT-1");
        var sub2 = CreateTransaction(reference: "ABC-PARENT-2");

        var debitAccount = AccountFactory.Create(
            "Debit",
            AccountType.Asset,
            new AccountIcon("favorite", "blue")
        );
        var creditAccount = AccountFactory.Create(
            "Credit",
            AccountType.Expense,
            new AccountIcon("favorite", "red")
        );
        var period = AccountingPeriod.Create("Period", default, default);
        var parentJournal = JournalEntry.Create(
            valueDate: default,
            description: "Parent split",
            amount: 10,
            debitAccount: debitAccount,
            creditAccount: creditAccount,
            accountingPeriod: period,
            transaction: parent
        );

        await using (var setup = CreateContext(options))
        {
            setup.Accounts.AddRange(debitAccount, creditAccount);
            setup.AccountingPeriods.Add(period);
            setup.Transactions.AddRange(parent, sub1, sub2);
            setup.JournalEntries.Add(parentJournal);
            await setup.SaveChangesAsync();
        }

        await using var actContext = CreateContext(options);
        var repo = new TransactionRepository(actContext);

        var outcome = await repo.CleanupBatchParentTransactions(dryRun: true, CancellationToken.None);

        outcome.DryRun.ShouldBeTrue();
        outcome.Parents.Count.ShouldBe(1);
        outcome.Parents[0].Parent.Id.ShouldBe(parent.Id);
        outcome.Parents[0].ChildCount.ShouldBe(2);
        outcome.Parents[0].JournalEntryCount.ShouldBe(1);

        await using var assertContext = CreateContext(options);
        (await assertContext.Transactions.FindAsync(parent.Id)).ShouldNotBeNull();
        (await assertContext.JournalEntries.FindAsync(parentJournal.Id)).ShouldNotBeNull();
    }

    [Fact]
    public async Task CleanupBatchParentTransactions_Execute_DeletesParentsAndJournalEntries_KeepsSubsAndUnrelated()
    {
        var options = new DbContextOptionsBuilder<KaesseliContext>()
            .UseInMemoryDatabase(databaseName: "CleanupBatchParents_Execute")
            .Options;

        var parent = CreateTransaction(reference: "ABC-PARENT");
        var sub1 = CreateTransaction(reference: "ABC-PARENT-1");
        var sub2 = CreateTransaction(reference: "ABC-PARENT-2");
        var unrelated = CreateTransaction(reference: "OTHER-REF");

        var debitAccount = AccountFactory.Create(
            "Debit",
            AccountType.Asset,
            new AccountIcon("favorite", "blue")
        );
        var creditAccount = AccountFactory.Create(
            "Credit",
            AccountType.Expense,
            new AccountIcon("favorite", "red")
        );
        var period = AccountingPeriod.Create("Period", default, default);
        var parentJournal1 = JournalEntry.Create(default, "split a", 1, debitAccount, creditAccount, period, parent);
        var parentJournal2 = JournalEntry.Create(default, "split b", 2, debitAccount, creditAccount, period, parent);
        var unrelatedJournal = JournalEntry.Create(default, "keep me", 3, debitAccount, creditAccount, period, unrelated);

        await using (var setup = CreateContext(options))
        {
            setup.Accounts.AddRange(debitAccount, creditAccount);
            setup.AccountingPeriods.Add(period);
            setup.Transactions.AddRange(parent, sub1, sub2, unrelated);
            setup.JournalEntries.AddRange(parentJournal1, parentJournal2, unrelatedJournal);
            await setup.SaveChangesAsync();
        }

        await using var actContext = CreateContext(options);
        var repo = new TransactionRepository(actContext);

        var outcome = await repo.CleanupBatchParentTransactions(dryRun: false, CancellationToken.None);

        outcome.DryRun.ShouldBeFalse();
        outcome.Parents.Count.ShouldBe(1);
        outcome.Parents[0].JournalEntryCount.ShouldBe(2);

        await using var assertContext = CreateContext(options);
        (await assertContext.Transactions.FindAsync(parent.Id)).ShouldBeNull();
        (await assertContext.Transactions.FindAsync(sub1.Id)).ShouldNotBeNull();
        (await assertContext.Transactions.FindAsync(sub2.Id)).ShouldNotBeNull();
        (await assertContext.Transactions.FindAsync(unrelated.Id)).ShouldNotBeNull();

        (await assertContext.JournalEntries.FindAsync(parentJournal1.Id)).ShouldBeNull();
        (await assertContext.JournalEntries.FindAsync(parentJournal2.Id)).ShouldBeNull();
        (await assertContext.JournalEntries.FindAsync(unrelatedJournal.Id)).ShouldNotBeNull();
    }

    [Fact]
    public async Task CleanupBatchParentTransactions_IgnoresOrphanSubsWithoutMatchingParent()
    {
        var options = new DbContextOptionsBuilder<KaesseliContext>()
            .UseInMemoryDatabase(databaseName: "CleanupBatchParents_OrphanSubs")
            .Options;

        var orphanSub = CreateTransaction(reference: "MISSING-PARENT-1");
        var unrelated = CreateTransaction(reference: "PLAIN-REF-WITH-NO-NUMERIC-CHILD");

        await using (var setup = CreateContext(options))
        {
            setup.Transactions.AddRange(orphanSub, unrelated);
            await setup.SaveChangesAsync();
        }

        await using var actContext = CreateContext(options);
        var repo = new TransactionRepository(actContext);

        var outcome = await repo.CleanupBatchParentTransactions(dryRun: false, CancellationToken.None);

        outcome.Parents.ShouldBeEmpty();

        await using var assertContext = CreateContext(options);
        (await assertContext.Transactions.FindAsync(orphanSub.Id)).ShouldNotBeNull();
        (await assertContext.Transactions.FindAsync(unrelated.Id)).ShouldNotBeNull();
    }

    [Fact]
    public async Task CleanupBatchParentTransactions_DoesNotTreatNonNumericSuffixAsChild()
    {
        var options = new DbContextOptionsBuilder<KaesseliContext>()
            .UseInMemoryDatabase(databaseName: "CleanupBatchParents_NonNumericSuffix")
            .Options;

        var notAParent = CreateTransaction(reference: "ABC");
        var notAChild = CreateTransaction(reference: "ABC-extra");

        await using (var setup = CreateContext(options))
        {
            setup.Transactions.AddRange(notAParent, notAChild);
            await setup.SaveChangesAsync();
        }

        await using var actContext = CreateContext(options);
        var repo = new TransactionRepository(actContext);

        var outcome = await repo.CleanupBatchParentTransactions(dryRun: false, CancellationToken.None);

        outcome.Parents.ShouldBeEmpty();
    }
}
