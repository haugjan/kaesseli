using Kaesseli.Domain.Accounts;

namespace Kaesseli.Application.Accounts;

public class GetAccountsQueryResult
{
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public string Type => TypeId.DisplayName();
    public required AccountType TypeId { get; init; }

    public required string Icon { get; init; }

    public required string IconColor { get; init; }
    // ReSharper restore UnusedAutoPropertyAccessor.Global
}