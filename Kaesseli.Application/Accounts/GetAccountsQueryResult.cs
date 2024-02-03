namespace Kaesseli.Application.Accounts;

public class GetAccountsQueryResult
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
}