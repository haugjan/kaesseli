namespace Kaesseli.Domain.Accounts;

public class AccountingPeriod
{
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public required Guid Id { get; init; }
    public required string Description { get; init; }

    public required DateOnly FromInclusive { get; init; }
    public required DateOnly ToInclusive { get; init; }
    // ReSharper restore UnusedAutoPropertyAccessor.Global
}