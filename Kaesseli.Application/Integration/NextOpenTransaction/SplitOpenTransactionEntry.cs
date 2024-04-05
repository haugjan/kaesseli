namespace Kaesseli.Application.Integration.NextOpenTransaction;

// ReSharper disable once ClassNeverInstantiated.Global
public class SplitOpenTransactionEntry
{
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public required Guid OtherAccountId { get; init; }
    public required decimal Amount { get; init; }
    // ReSharper restore UnusedAutoPropertyAccessor.Global
}