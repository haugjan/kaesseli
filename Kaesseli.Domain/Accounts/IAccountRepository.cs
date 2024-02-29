namespace Kaesseli.Domain.Accounts;

public interface IAccountRepository
{
    Task<Account> AddAccount(Account account, CancellationToken cancellationToken);
    Task<IEnumerable<Account>> GetAccounts(CancellationToken cancellationToken);
    Task<IEnumerable<Account>> GetAccounts(AccountType accountType, CancellationToken cancellationToken);
    Task<Account> GetAccount(Guid accountId, CancellationToken cancellationToken);
}