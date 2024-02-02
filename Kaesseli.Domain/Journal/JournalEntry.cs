using Kaesseli.Domain.Common;
namespace Kaesseli.Domain.Journal;


public class JournalEntry
{
    public required Guid Id { get; init; }
    private Account? _account;
    public required DateOnly ValueDate { get; init; }

    public required string Description { get; init; }
    public required decimal Amount { get; init; }
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