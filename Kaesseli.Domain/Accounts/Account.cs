namespace Kaesseli.Domain.Accounts;

public class Account
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required AccountType Type { get; init; }
}