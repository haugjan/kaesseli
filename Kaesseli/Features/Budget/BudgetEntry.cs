using Kaesseli.Features.Accounts;

namespace Kaesseli.Features.Budget;

public class BudgetEntry
{
    private BudgetEntry() { }

    public Guid Id { get; private init; }
    public string Description { get; private set; } = null!;
    public decimal Amount { get; private set; }
    public Account Account { get; private init; } = null!;
    public AccountingPeriod AccountingPeriod { get; private init; } = null!;

    public static BudgetEntry Create(string description, decimal amount, Account account, AccountingPeriod accountingPeriod)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        ArgumentNullException.ThrowIfNull(account);
        ArgumentNullException.ThrowIfNull(accountingPeriod);

        if (account.Type is AccountType.Asset or AccountType.Liability)
            throw new BudgetNotAllowedException(account.Type);

        return new BudgetEntry
        {
            Id = Guid.NewGuid(),
            Description = description,
            Amount = amount,
            Account = account,
            AccountingPeriod = accountingPeriod,
        };
    }

    public void UpdateBudget(decimal amount, string description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        Amount = amount;
        Description = description;
    }

    public override string ToString() =>
        $"Budget {Account.Name}: {Amount:C}";
}
