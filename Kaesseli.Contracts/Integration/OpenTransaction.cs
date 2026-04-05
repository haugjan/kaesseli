using Kaesseli.Contracts.Accounts;

namespace Kaesseli.Contracts.Integration;

public record OpenTransaction(
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
