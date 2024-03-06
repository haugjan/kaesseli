namespace Kaesseli.Domain.Accounts;

public static class AccountTypeExtensions
{
    // ReSharper disable StringLiteralTypo
    public static string DisplayName(this AccountType accountType) =>
        accountType switch
        {
            AccountType.Asset => "Aktiv",
            AccountType.Liability => "Passiv",
            AccountType.Revenue => "Einkommen",
            AccountType.Expense => "Ausgaben",
            _ => throw new ArgumentOutOfRangeException(paramName: nameof(accountType), accountType, message: null)
        };

    // ReSharper restore StringLiteralTypo
}