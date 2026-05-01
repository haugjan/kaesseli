using System.Text.RegularExpressions;

namespace Kaesseli.Features.Accounts;

public partial class Account
{
    [GeneratedRegex(@"^[1-4]\d{3}$")]
    private static partial Regex NumberRegex();

    [GeneratedRegex(@"^[a-z0-9-]{2,20}$")]
    private static partial Regex ShortNameRegex();

    private Account() { }

    public Guid Id { get; private init; }
    public string Number { get; private set; } = null!;
    public string ShortName { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public AccountType Type { get; private set; }
    public AccountIcon Icon { get; private set; } = null!;

    public static Account Create(
        string name,
        AccountType type,
        string number,
        string shortName,
        AccountIcon icon
    )
    {
        Validate(name, type, number, shortName, icon);

        return new Account
        {
            Id = Guid.NewGuid(),
            Name = name,
            Type = type,
            Number = number,
            ShortName = shortName,
            Icon = icon,
        };
    }

    public void Update(
        string name,
        AccountType type,
        string number,
        string shortName,
        AccountIcon icon
    )
    {
        Validate(name, type, number, shortName, icon);

        Name = name;
        Type = type;
        Number = number;
        ShortName = shortName;
        Icon = icon;
    }

    private static void Validate(
        string name,
        AccountType type,
        string number,
        string shortName,
        AccountIcon icon
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(icon);

        if (string.IsNullOrEmpty(number) || !NumberRegex().IsMatch(number))
            throw new InvalidAccountNumberException(number);

        var expectedFirstDigit = type switch
        {
            AccountType.Asset => '1',
            AccountType.Liability => '2',
            AccountType.Revenue => '3',
            AccountType.Expense => '4',
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
        };
        if (number[0] != expectedFirstDigit)
            throw new AccountNumberDoesNotMatchTypeException(number, type);

        if (string.IsNullOrEmpty(shortName) || !ShortNameRegex().IsMatch(shortName))
            throw new InvalidAccountShortNameException(shortName);
    }

    public override string ToString() => $"{Number} {Name} ({Type.DisplayName()})";
}
