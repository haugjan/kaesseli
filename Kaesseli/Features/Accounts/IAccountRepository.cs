namespace Kaesseli.Features.Accounts;

public interface IAccountRepository
{
    Task<Account> AddAccount(Account account, CancellationToken cancellationToken);
    Task<IEnumerable<Account>> GetAccounts(CancellationToken cancellationToken);
    Task<IEnumerable<Account>> GetAccounts(AccountType accountType, CancellationToken cancellationToken);
    Task<Account> GetAccount(Guid accountId, CancellationToken cancellationToken);
    Task<AccountingPeriod> GetAccountingPeriod(Guid accountingPeriodId, CancellationToken cancellationToken);
    Task<AccountingPeriod> AddAccountingPeriod(AccountingPeriod accountingPeriod, CancellationToken cancellationToken);
    Task<IEnumerable<AccountingPeriod>> GetAccountingPeriods(CancellationToken cancellationToken);
}