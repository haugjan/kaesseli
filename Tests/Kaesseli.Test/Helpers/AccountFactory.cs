using Kaesseli.Features.Accounts;

namespace Kaesseli.Test.Helpers;

internal static class AccountFactory
{
    public static Account Create(string name, AccountType type, AccountIcon icon) =>
        Account.Create(name, type, DefaultNumber(type), DefaultShortName(type), icon);

    public static string DefaultNumber(AccountType type) =>
        type switch
        {
            AccountType.Asset => "1000",
            AccountType.Liability => "2000",
            AccountType.Revenue => "3000",
            AccountType.Expense => "4000",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
        };

    public static string DefaultShortName(AccountType type) => type.ToString().ToLowerInvariant();
}
