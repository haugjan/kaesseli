namespace Kaesseli.Domain.Accounts;

public static class AccountTypeExtensions
{
    // ReSharper disable StringLiteralTypo
    public static string DisplayName(this AccountType accountType) =>
        accountType switch
        {
            AccountType.Asset => "Aktiv",
            AccountType.Liability => "Passiv",
            AccountType.Revenue => "Aufwand",
            AccountType.Expense => "Ertrag",
            _ => throw new ArgumentOutOfRangeException(paramName: nameof(accountType), accountType, message: null)
        };
    // ReSharper restore StringLiteralTypo
}