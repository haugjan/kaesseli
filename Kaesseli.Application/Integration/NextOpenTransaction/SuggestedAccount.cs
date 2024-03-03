using Kaesseli.Domain.Accounts;

namespace Kaesseli.Application.Integration.NextOpenTransaction;

public class SuggestedAccount
{
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public required double Relevance { get; init; }
    public required Guid AccountId { get; init; }
    public required string AccountName { get; init; }
    public required string AccountType { get; init; }
    public required AccountType AccountTypeId { get; init; }
    public required string AccountIcon { get; init; }
    public required string AccountIconColor { get; init; }
    // ReSharper restore UnusedAutoPropertyAccessor.Global
}