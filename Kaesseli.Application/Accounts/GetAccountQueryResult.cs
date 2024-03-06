using Kaesseli.Domain.Accounts;

namespace Kaesseli.Application.Accounts;

public class GetAccountQueryResult
{
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public required Guid Id { get; init;}
    public required string Name { get; init;}
    public required string Icon { get; init; }
    public required string IconColor { get; init; }
    public required string Type { get; init; }
    public required AccountType TypeId { get; init; }
    // ReSharper disable once UnusedMember.Global
    public required decimal AccountBalance { get; init; }
    public required decimal? Budget { get; init; }
    public required decimal? BudgetBalance { get; init; }
    public required IEnumerable<GetAccountQueryResultEntry> Entries { get; init; }
    // ReSharper restore UnusedAutoPropertyAccessor.Global
}