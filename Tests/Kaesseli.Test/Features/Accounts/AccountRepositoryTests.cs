using Kaesseli.Features.Accounts;
using Kaesseli.Features.Budget;
using Kaesseli.Features.Journal;
using Kaesseli.Infrastructure;
using Kaesseli.Test.Faker;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace Kaesseli.Test.Features.Accounts;

public class AccountRepositoryTests
{
    private static KaesseliContext CreateContext(DbContextOptions<KaesseliContext> options)
    {
        var timeProvider = TimeProvider.System;
        return new(options, timeProvider);
    }

    [Fact]
    public async Task AddAccount_ShouldCorrectlyAddAccount()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<KaesseliContext>()
            .UseInMemoryDatabase(databaseName: "AddAccountDb")
            .Options;

        var account = AccountFactory.Create(
            "Test Account",
            AccountType.Asset,
            new AccountIcon("favorite", "blue")
        );

        await using var context = CreateContext(options);
        var repository = new AccountRepository(context);

        // Act
        var result = await repository.AddAccount(account, CancellationToken.None);

        // Assert
        Assert.Equivalent(account, result);

        await using var assertContext = CreateContext(options);
        var addedAccount = await assertContext.Accounts.FindAsync(account.Id);
        Assert.Equivalent(account, addedAccount);
    }

    [Fact]
    public async Task GetAccounts_ShouldReturnAllAccounts()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<KaesseliContext>()
            .UseInMemoryDatabase(databaseName: "GetAllAccountsDb")
            .Options;
        var cancellationToken = new CancellationToken();

        await using var setupContext = CreateContext(options);
        setupContext.Accounts.Add(
            entity: AccountFactory.Create(
                "Account 1",
                AccountType.Expense,
                new AccountIcon("favorite", "blue")
            )
        );
        setupContext.Accounts.Add(
            entity: AccountFactory.Create(
                "Account 2",
                AccountType.Expense,
                new AccountIcon("favorite", "blue")
            )
        );
        await setupContext.SaveChangesAsync(cancellationToken);

        var repository = new AccountRepository(setupContext);

        // Act
        var accounts = await repository.GetAccounts(cancellationToken);

        // Assert
        accounts.Count().ShouldBe(2);
    }

    [Fact]
    public async Task GetAccountsOfType_ShouldReturnAccountsOfType()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<KaesseliContext>()
            .UseInMemoryDatabase(databaseName: "GetAccountsDb")
            .Options;
        var cancellationToken = new CancellationToken();

        await using var setupContext = CreateContext(options);
        var accounts = new List<Account>
        {
            AccountFactory.Create(
                "Account 1",
                AccountType.Asset,
                new AccountIcon("favorite", "blue")
            ),
            AccountFactory.Create(
                "Account 2",
                AccountType.Expense,
                new AccountIcon("favorite", "blue")
            ),
            AccountFactory.Create(
                "Account 3",
                AccountType.Asset,
                new AccountIcon("favorite", "blue")
            ),
        };
        setupContext.Accounts.AddRange(accounts);
        await setupContext.SaveChangesAsync(cancellationToken);

        var repository = new AccountRepository(setupContext);

        // Act
        var currentAccounts = (
            await repository.GetAccounts(AccountType.Asset, cancellationToken)
        ).ToArray();

        // Assert
        currentAccounts.Length.ShouldBe(2);
        Assert.Equivalent(
            accounts.Where(account => account.Type == AccountType.Asset),
            currentAccounts
        );
    }

    [Fact]
    public async Task GetAccount_ShouldReturnAccountWhenExists()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<KaesseliContext>()
            .UseInMemoryDatabase(databaseName: "GetAccountDb")
            .Options;

        var account = AccountFactory.Create(
            "Existing Account",
            AccountType.Liability,
            new AccountIcon("favorite", "blue")
        );

        await using var setupContext = CreateContext(options);
        setupContext.Accounts.Add(account);
        await setupContext.SaveChangesAsync();

        var repository = new AccountRepository(setupContext);

        // Act
        var result = await repository.GetAccount(account.Id, CancellationToken.None);

        // Assert
        Assert.Equivalent(account, result);
    }

    [Fact]
    public async Task GetNotExistingAccount_ShouldThrowException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<KaesseliContext>()
            .UseInMemoryDatabase(databaseName: "GetAccountNotExistDb")
            .Options;

        var repository = new AccountRepository(context: CreateContext(options));

        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(() =>
            repository.GetAccount(accountId: Guid.NewGuid(), CancellationToken.None)
        );
    }

    [Fact]
    public async Task GetAccountingPeriods_ShouldReturnAllAccountingPeriods()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<KaesseliContext>()
            .UseInMemoryDatabase(databaseName: "GetAccountingPeriodsDb")
            .Options;
        var cancellationToken = new CancellationToken();

        await using var setupContext = CreateContext(options);
        var expectedPeriods = new SmartFaker<AccountingPeriod>().Generate(count: 5);

        setupContext.AccountingPeriods.AddRange(expectedPeriods);
        await setupContext.SaveChangesAsync(cancellationToken);

        var repository = new AccountRepository(setupContext);

        // Act
        var currentPeriods = await repository.GetAccountingPeriods(cancellationToken);

        // Assert
        Assert.Equivalent(expectedPeriods, currentPeriods);
    }

    [Fact]
    public async Task GetAccountingPeriod_ShouldReturnAllAccountingPeriods()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<KaesseliContext>()
            .UseInMemoryDatabase(databaseName: "GetAccountingPeriodDb")
            .Options;
        var cancellationToken = new CancellationToken();

        await using var setupContext = CreateContext(options);
        var periods = new SmartFaker<AccountingPeriod>().Generate(count: 5);
        var expectedPeriod = periods[index: 1];

        setupContext.AccountingPeriods.AddRange(periods);
        await setupContext.SaveChangesAsync(cancellationToken);

        var repository = new AccountRepository(setupContext);

        // Act
        var currentPeriod = await repository.GetAccountingPeriod(
            expectedPeriod.Id,
            cancellationToken
        );

        // Assert
        Assert.Equivalent(expectedPeriod, currentPeriod);
    }

    [Fact]
    public async Task GetNotExistingAccountingPeriod_ShouldThrowException()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<KaesseliContext>()
            .UseInMemoryDatabase(databaseName: "GetNotExistingAccountingPeriod")
            .Options;
        var cancellationToken = new CancellationToken();

        await using var setupContext = CreateContext(options);
        var repository = new AccountRepository(setupContext);

        // Act & Assert
        await Should.ThrowAsync<EntityNotFoundException>(async () =>
            await repository.GetAccountingPeriod(
                accountingPeriodId: Guid.NewGuid(),
                cancellationToken
            )
        );
    }

    [Fact]
    public async Task DeleteAccount_ShouldThrow_WhenJournalEntriesReferenceAccount()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<KaesseliContext>()
            .UseInMemoryDatabase(databaseName: "DeleteAccountWithJournalEntries")
            .Options;

        var accountToDelete = AccountFactory.Create(
            "Asset",
            AccountType.Asset,
            new AccountIcon("favorite", "blue")
        );
        var otherAccount = AccountFactory.Create(
            "Expense",
            AccountType.Expense,
            new AccountIcon("favorite", "red")
        );
        var period = AccountingPeriod.Create("Test Period", default, default);
        var journalEntry = JournalEntry.Create(
            valueDate: default,
            description: "test",
            amount: 100,
            debitAccount: accountToDelete,
            creditAccount: otherAccount,
            accountingPeriod: period
        );

        await using var setupContext = CreateContext(options);
        setupContext.Accounts.AddRange(accountToDelete, otherAccount);
        setupContext.AccountingPeriods.Add(period);
        setupContext.JournalEntries.Add(journalEntry);
        await setupContext.SaveChangesAsync();

        var repository = new AccountRepository(setupContext);

        // Act & Assert
        await Should.ThrowAsync<AccountInUseException>(() =>
            repository.DeleteAccount(accountToDelete.Id, CancellationToken.None)
        );

        await using var assertContext = CreateContext(options);
        (await assertContext.Accounts.FindAsync(accountToDelete.Id)).ShouldNotBeNull();
    }

    [Fact]
    public async Task DeleteAccount_ShouldRemoveBudgetEntries_WhenNoJournalEntriesExist()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<KaesseliContext>()
            .UseInMemoryDatabase(databaseName: "DeleteAccountWithBudgetEntries")
            .Options;

        var account = AccountFactory.Create(
            "Expense",
            AccountType.Expense,
            new AccountIcon("favorite", "blue")
        );
        var period = AccountingPeriod.Create("Test Period", default, default);
        var budgetEntry = BudgetEntry.Create(
            description: "test budget",
            amount: 50,
            account: account,
            accountingPeriod: period
        );

        await using var setupContext = CreateContext(options);
        setupContext.Accounts.Add(account);
        setupContext.AccountingPeriods.Add(period);
        setupContext.BudgetEntries.Add(budgetEntry);
        await setupContext.SaveChangesAsync();

        var repository = new AccountRepository(setupContext);

        // Act
        await repository.DeleteAccount(account.Id, CancellationToken.None);

        // Assert
        await using var assertContext = CreateContext(options);
        (await assertContext.Accounts.FindAsync(account.Id)).ShouldBeNull();
        (await assertContext.BudgetEntries.FindAsync(budgetEntry.Id)).ShouldBeNull();
    }

    [Fact]
    public async Task CleanupOrphanedAccountReferences_ShouldRemoveOrphans_AndKeepValidEntries()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<KaesseliContext>()
            .UseInMemoryDatabase(databaseName: "CleanupOrphanedAccountReferences")
            .Options;

        var aliveAccount = AccountFactory.Create(
            "Alive",
            AccountType.Expense,
            new AccountIcon("favorite", "blue")
        );
        var otherAlive = AccountFactory.Create(
            "OtherAlive",
            AccountType.Asset,
            new AccountIcon("favorite", "green")
        );
        var ghostAccount = AccountFactory.Create(
            "Ghost",
            AccountType.Asset,
            new AccountIcon("favorite", "red")
        );
        var period = AccountingPeriod.Create("Test Period", default, default);

        var validJournal = JournalEntry.Create(
            valueDate: default,
            description: "valid",
            amount: 10,
            debitAccount: aliveAccount,
            creditAccount: otherAlive,
            accountingPeriod: period
        );
        var orphanJournal = JournalEntry.Create(
            valueDate: default,
            description: "orphan",
            amount: 20,
            debitAccount: aliveAccount,
            creditAccount: ghostAccount,
            accountingPeriod: period
        );

        var validBudget = BudgetEntry.Create(
            description: "valid budget",
            amount: 30,
            account: aliveAccount,
            accountingPeriod: period
        );
        var orphanBudget = BudgetEntry.Create(
            description: "orphan budget",
            amount: 40,
            account: AccountFactory.Create(
                "GhostBudget",
                AccountType.Expense,
                new AccountIcon("favorite", "red")
            ),
            accountingPeriod: period
        );

        await using (var setupContext = CreateContext(options))
        {
            setupContext.Accounts.AddRange(aliveAccount, otherAlive, ghostAccount);
            setupContext.AccountingPeriods.Add(period);
            setupContext.JournalEntries.AddRange(validJournal, orphanJournal);
            setupContext.BudgetEntries.AddRange(validBudget, orphanBudget);
            await setupContext.SaveChangesAsync();
        }

        // Use a fresh context to remove the ghost accounts without the
        // InMemory ChangeTracker also tracking the dependent entries.
        await using (var removeContext = CreateContext(options))
        {
            var ghostIds = new[] { ghostAccount.Id, orphanBudget.Account.Id };
            var ghosts = await removeContext
                .Accounts.Where(a => ghostIds.Contains(a.Id))
                .ToListAsync();
            removeContext.Accounts.RemoveRange(ghosts);
            await removeContext.SaveChangesAsync();
        }

        await using var actContext = CreateContext(options);
        var repository = new AccountRepository(actContext);

        // Act
        var result = await repository.CleanupOrphanedAccountReferences(CancellationToken.None);

        // Assert
        result.JournalEntriesDeleted.ShouldBe(1);
        result.BudgetEntriesDeleted.ShouldBe(1);

        await using var assertContext = CreateContext(options);
        (await assertContext.JournalEntries.FindAsync(validJournal.Id)).ShouldNotBeNull();
        (await assertContext.JournalEntries.FindAsync(orphanJournal.Id)).ShouldBeNull();
        (await assertContext.BudgetEntries.FindAsync(validBudget.Id)).ShouldNotBeNull();
        (await assertContext.BudgetEntries.FindAsync(orphanBudget.Id)).ShouldBeNull();
    }
}
