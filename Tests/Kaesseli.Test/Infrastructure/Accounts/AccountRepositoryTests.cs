using Kaesseli.Infrastructure;
using Kaesseli.Features.Accounts;
using Kaesseli.Test.Faker;
using Microsoft.EntityFrameworkCore;
using Moq;
using Shouldly;
using Xunit;

namespace Kaesseli.Test.Infrastructure.Accounts;

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

        var account = new Account
        {
            Id = Guid.NewGuid(),
            Name = "Test Account",
            Type = AccountType.Asset,
            Icon = new AccountIcon("favorite", "blue"),
        };

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
            entity: new Account
            {
                Id = Guid.NewGuid(),
                Name = "Account 1",
                Type = AccountType.Expense,
                Icon = new AccountIcon("favorite", "blue"),
            }
        );
        setupContext.Accounts.Add(
            entity: new Account
            {
                Id = Guid.NewGuid(),
                Name = "Account 2",
                Type = AccountType.Expense,
                Icon = new AccountIcon("favorite", "blue"),
            }
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
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Account 1",
                Type = AccountType.Asset,
                Icon = new AccountIcon("favorite", "blue"),
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Account 2",
                Type = AccountType.Expense,
                Icon = new AccountIcon("favorite", "blue"),
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Account 3",
                Type = AccountType.Asset,
                Icon = new AccountIcon("favorite", "blue"),
            },
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
        Assert.Equivalent(accounts.Where(account => account.Type == AccountType.Asset), currentAccounts);
    }

    [Fact]
    public async Task GetAccount_ShouldReturnAccountWhenExists()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<KaesseliContext>()
            .UseInMemoryDatabase(databaseName: "GetAccountDb")
            .Options;

        var account = new Account
        {
            Id = Guid.NewGuid(),
            Name = "Existing Account",
            Type = AccountType.Liability,
            Icon = new AccountIcon("favorite", "blue"),
        };

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
}
