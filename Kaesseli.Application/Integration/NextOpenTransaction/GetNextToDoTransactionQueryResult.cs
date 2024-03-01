using Kaesseli.Domain.Accounts;

namespace Kaesseli.Application.Integration.NextOpenTransaction;

public class GetNextOpenTransactionQueryResult
{
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public required Guid TransactionId { get; init; }
    public required decimal Amount { get; init; }
    public required string Description { get; init; }
    public required DateOnly ValueDate { get; init; }
    public required string AccountName { get; init; }
    public required string AccountType { get; init; }
    public required AccountType AccountTypeId { get; init; }

    public required IEnumerable<SuggestedAccount> SuggestedAccounts { get; init; }
    // ReSharper restore UnusedAutoPropertyAccessor.Global
}