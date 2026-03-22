using Kaesseli.Domain.Accounts;

namespace Kaesseli.Application.Accounts;

public class GetAccountsQuery
{
    public AccountType? AccountType { get; init; }
}