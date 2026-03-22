namespace Kaesseli.Application.Accounts;

// ReSharper disable once ClassNeverInstantiated.Global
public class AddAccountingPeriodCommand
{
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public required DateOnly FromInclusive { get; init; }
    public required DateOnly ToInclusive { get; init; }
    public required string? Description { get; init; }
    // ReSharper restore UnusedAutoPropertyAccessor.Global
}