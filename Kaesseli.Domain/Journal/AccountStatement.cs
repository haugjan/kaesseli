using Kaesseli.Domain.Accounts;

namespace Kaesseli.Domain.Journal;

public class AccountStatement
{
    public required Guid Id { get; init; }
    public required Account Account { get; init; }
    public required decimal BalanceBefore { get; init; }
    public required decimal BalanceAfter { get; init; }
    public required DateOnly ValueDateFrom { get; init; }
    public required DateOnly ValueDateTo { get; init; }
    public required string Reference { get; init; }
    public required IEnumerable<PaymentEntry> PaymentEntries { get; init; }

}