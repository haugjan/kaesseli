using Kaesseli.Domain.Common;
using Kaesseli.Domain.Journal;

namespace Kaesseli.Domain.Budget;

public class BudgetEntry
{
    private Account? _account;
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset ValueDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }

    public Account? Account
    {
        get => _account;
        set
        {
            ThrowIfAccountAlreadySet(value);
            _account = value;
        }
    }

    private void ThrowIfAccountAlreadySet(Account? value)
    {
        if (_account is not null && _account?.Id != value?.Id)
            throw new JournalEntriesImmutableException();
    }
}