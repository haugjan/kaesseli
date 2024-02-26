using Kaesseli.Domain.Accounts;

namespace Kaesseli.Domain.Integration;

public class TransactionSummary
{
    public required Guid Id { get; init; }
    public required Account Account { get; init; }
    public required decimal BalanceBefore { get; init; }
    public required decimal BalanceAfter { get; init; }
    public required DateOnly ValueDateFrom { get; init; }
    public required DateOnly ValueDateTo { get; init; }
    public required string Reference { get; init; }
    public required IEnumerable<Transaction> Transactions { get; init; }

}