namespace Kaesseli.Infrastructure.Budget;

public class AccountNotFoundException(Guid accountId) : 
    Exception($"Account with id {accountId} not found.");