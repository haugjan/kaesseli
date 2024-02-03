using Kaesseli.Domain.Common;

namespace Kaesseli.Domain.Accounts;

public interface IAccountRepository
{
    Task<Account> AddAccount(Account account, CancellationToken cancellationToken);
    Task<IEnumerable<Account>> GetAccounts(CancellationToken cancellationToken);
    Task<Account> GetAccount(Guid accountId, CancellationToken cancellationToken);
}