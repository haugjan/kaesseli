using MediatR;

namespace Kaesseli.Application.Integration.NextOpenTransaction;

// ReSharper disable once ClassNeverInstantiated.Global
public class SplitOpenTransactionCommand : IRequest
{
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public required Guid AccountingPeriodId { get; init; }
    public required Guid TransactionId { get; init; }
    public required IEnumerable<SplitOpenTransactionEntry> Entries { get; init; }
    
    // ReSharper restore UnusedAutoPropertyAccessor.Global
}
