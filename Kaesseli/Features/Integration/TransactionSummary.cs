using Kaesseli.Features.Accounts;

namespace Kaesseli.Features.Integration;

public class TransactionSummary
{
    private TransactionSummary() { }

    public Guid Id { get; private init; }
    public Account Account { get; private init; } = null!;
    public decimal BalanceBefore { get; private init; }
    public decimal BalanceAfter { get; private init; }
    public DateOnly ValueDateFrom { get; private init; }
    public DateOnly ValueDateTo { get; private init; }
    public string Reference { get; private init; } = null!;
    public IEnumerable<Transaction> Transactions { get; private init; } = null!;

    public static TransactionSummary Create(
        Account account,
        decimal balanceBefore,
        decimal balanceAfter,
        DateOnly valueDateFrom,
        DateOnly valueDateTo,
        string reference,
        IEnumerable<Transaction> transactions)
    {
        ArgumentNullException.ThrowIfNull(account);

        return new TransactionSummary
        {
            Id = Guid.NewGuid(),
            Account = account,
            BalanceBefore = balanceBefore,
            BalanceAfter = balanceAfter,
            ValueDateFrom = valueDateFrom,
            ValueDateTo = valueDateTo,
            Reference = reference,
            Transactions = transactions,
        };
    }
}
