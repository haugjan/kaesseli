using Kaesseli.Domain.Common;

namespace Kaesseli.Application.Accounts;

public class GetAccountsQueryResult
{
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public required Guid Id { get; init; }
    public required string Name { get; init; }

    public required AccountType Type { get; init; }
    // ReSharper restore UnusedAutoPropertyAccessor.Global
}