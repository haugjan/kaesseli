namespace Kaesseli.Domain.Accounts;

public static class AccountTypeExtensions
{
    public static ParentAccountType ToParentAccountType(this AccountType accountType) =>
        accountType is AccountType.Asset or AccountType.Liability
            ? ParentAccountType.BalanceSheet
            : ParentAccountType.IncomeStatement;

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

    public static string DisplayName(this ParentAccountType parentAccountType) =>
        parentAccountType switch
        {
            ParentAccountType.BalanceSheet => "Bilanzkonto", 
            ParentAccountType.IncomeStatement => "Erfolgskonto", 
            _ => throw new ArgumentOutOfRangeException(paramName: nameof(parentAccountType), parentAccountType, message: null)
        };
    // ReSharper restore StringLiteralTypo
}