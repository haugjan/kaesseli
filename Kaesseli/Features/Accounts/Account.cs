namespace Kaesseli.Features.Accounts;

public class Account
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required AccountType Type { get; init; }
    public required AccountIcon Icon { get; init; }

    public override string ToString() =>
        $"{Name} ({Type.DisplayName()})";
}
