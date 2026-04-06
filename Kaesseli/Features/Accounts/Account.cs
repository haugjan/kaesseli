namespace Kaesseli.Features.Accounts;

public class Account
{
    private Account() { }

    public Guid Id { get; private init; }
    public string Name { get; private set; } = null!;
    public AccountType Type { get; private set; }
    public AccountIcon Icon { get; private set; } = null!;

    public static Account Create(string name, AccountType type, AccountIcon icon)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(icon);

        return new Account
        {
            Id = Guid.NewGuid(),
            Name = name,
            Type = type,
            Icon = icon,
        };
    }

    public void Update(string name, AccountType type, AccountIcon icon)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(icon);

        Name = name;
        Type = type;
        Icon = icon;
    }

    public override string ToString() =>
        $"{Name} ({Type.DisplayName()})";
}
