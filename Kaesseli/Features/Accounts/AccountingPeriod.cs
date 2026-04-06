namespace Kaesseli.Features.Accounts;

public class AccountingPeriod
{
    private AccountingPeriod() { }

    public Guid Id { get; private init; }
    public string Description { get; private set; } = null!;
    public DateOnly FromInclusive { get; private set; }
    public DateOnly ToInclusive { get; private set; }

    public static AccountingPeriod Create(string description, DateOnly fromInclusive, DateOnly toInclusive)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(fromInclusive, toInclusive);

        return new AccountingPeriod
        {
            Id = Guid.NewGuid(),
            Description = description,
            FromInclusive = fromInclusive,
            ToInclusive = toInclusive,
        };
    }

    public void Update(string description, DateOnly fromInclusive, DateOnly toInclusive)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(fromInclusive, toInclusive);

        Description = description;
        FromInclusive = fromInclusive;
        ToInclusive = toInclusive;
    }
}
