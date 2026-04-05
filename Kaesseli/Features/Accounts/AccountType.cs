using Kaesseli.Contracts.Accounts;

namespace Kaesseli.Features.Accounts;

public static class AccountTypeExtensions
{
    extension(AccountType accountType)
    {
        // ReSharper disable StringLiteralTypo
        public string DisplayName() =>
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
}
