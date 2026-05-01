namespace Kaesseli.Features.Accounts;

public class AccountInUseException(Guid accountId)
    : Exception($"Account {accountId} cannot be deleted because journal entries reference it.")
{
    public Guid AccountId { get; } = accountId;
}
