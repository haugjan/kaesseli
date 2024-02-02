namespace Kaesseli.Infrastructure.Common;

public class AccountNotFoundException(Guid accountId) :
    Exception(message: $"Account with id {accountId} not found.");