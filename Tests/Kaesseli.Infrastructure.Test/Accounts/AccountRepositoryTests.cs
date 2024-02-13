using FluentAssertions;
using Kaesseli.Domain.Accounts;
using Kaesseli.Infrastructure.Accounts;
using Kaesseli.Infrastructure.Common;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Kaesseli.Infrastructure.Test.Accounts;

public class AccountRepositoryTests
{
    private static KaesseliContext CreateContext(DbContextOptions<KaesseliContext> options) =>
        new(options);

    [Fact]
    public async Task AddAccount_ShouldCorrectlyAddAccount()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<KaesseliContext>()
                      .UseInMemoryDatabase(databaseName: "AddAccountDb")
                      .Options;

        var account = new Account { Id = Guid.NewGuid(), Name = "Test Account", Type = AccountType.Asset };

        await using var context = CreateContext(options);
        var repository = new AccountRepository(context);

        // Act
        var result = await repository.AddAccount(account, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(account);

        await using var assertContext = CreateContext(options);
        var addedAccount = await assertContext.Accounts.FindAsync(account.Id);
        addedAccount.Should().BeEquivalentTo(account);
    }

    [Fact]
    public async Task GetAccounts_ShouldReturnAllAccounts()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<KaesseliContext>()
                      .UseInMemoryDatabase(databaseName: "GetAccountsDb")
                      .Options;

        await using var setupContext = CreateContext(options);
        setupContext.Accounts.Add(entity: new Account { Id = Guid.NewGuid(), Name = "Account 1", Type = AccountType.Expense });
        setupContext.Accounts.Add(entity: new Account { Id = Guid.NewGuid(), Name = "Account 2", Type = AccountType.Expense });
        await setupContext.SaveChangesAsync();

        var repository = new AccountRepository(setupContext);

        // Act
        var accounts = await repository.GetAccounts(CancellationToken.None);

        // Assert
        accounts.Should().HaveCount(expected: 2);
    }

    [Fact]
    public async Task GetAccount_ShouldReturnAccountWhenExists()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<KaesseliContext>()
                      .UseInMemoryDatabase(databaseName: "GetAccountDb")
                      .Options;

        var account = new Account { Id = Guid.NewGuid(), Name = "Existing Account", Type = AccountType.Liability };

        await using var setupContext = CreateContext(options);
        setupContext.Accounts.Add(account);
        await setupContext.SaveChangesAsync();

        var repository = new AccountRepository(setupContext);

        // Act
        var result = await repository.GetAccount(account.Id, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(account);
    }

    [Fact]
    public async Task GetAccount_ShouldThrowWhenAccountDoesNotExist()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<KaesseliContext>()
                      .UseInMemoryDatabase(databaseName: "GetAccountNotExistDb")
                      .Options;

        var repository = new AccountRepository(context: CreateContext(options));

        // Act & Assert
        await Assert.ThrowsAsync<AccountNotFoundException>(
            () => repository.GetAccount(accountId: Guid.NewGuid(), CancellationToken.None));
    }
}