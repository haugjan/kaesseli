namespace Kaesseli.Contracts.Features.Accounts;

public static class GetAccountsContract
{
    public record Result(Guid Id, string Name, AccountType TypeId, string Icon, string IconColor);
}
