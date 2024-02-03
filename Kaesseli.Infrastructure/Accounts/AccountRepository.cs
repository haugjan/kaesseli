using Kaesseli.Domain.Accounts;
using Kaesseli.Domain.Common;
using Kaesseli.Infrastructure.Common;
using Microsoft.EntityFrameworkCore;

namespace Kaesseli.Infrastructure.Accounts;

internal class AccountRepository(KaesseliContext context) : IAccountRepository
{
    public async Task<Account> AddAccount(Account account, CancellationToken cancellationToken)
    {
        context.Accounts.Add(account);
        await context.SaveChangesAsync(cancellationToken);
        return account;
    }

    public async Task<IEnumerable<Account>> GetAccounts(CancellationToken cancellationToken) =>
        await context.Accounts.ToListAsync(cancellationToken);

    public async Task<Account> GetAccount(Guid accountId, CancellationToken cancellationToken) =>
        await context.Accounts.SingleOrDefaultAsync(account => account.Id == accountId, cancellationToken)
     ?? throw new AccountNotFoundException(accountId);
}