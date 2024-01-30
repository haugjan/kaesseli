using Kaesseli.Domain.Exceptions;
namespace Kaesseli.Domain.Entities;


public class JournalEntry
{
    public Guid Id { get; set; }
    public DateTimeOffset ValueDate { get; set; }
    private Account? _account;

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