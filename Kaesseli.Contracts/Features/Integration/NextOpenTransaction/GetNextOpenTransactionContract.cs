using Kaesseli.Contracts.Features.Accounts;

namespace Kaesseli.Contracts.Features.Integration.NextOpenTransaction;

public static class GetNextOpenTransactionContract
{
    public record Result(
        Guid Id,
        decimal Amount,
        string Description,
        DateOnly ValueDate,
        string AccountName,
        string AccountType,
        AccountType AccountTypeId,
        IEnumerable<SuggestedAccount> SuggestedAccounts);

    public record SuggestedAccount(
        double Relevance,
        Guid AccountId,
        string AccountName,
        string AccountType,
        AccountType AccountTypeId,
        string AccountIcon,
        string AccountIconColor);
}
