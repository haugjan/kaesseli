using MediatR;

namespace Kaesseli.Application.Integration.NextOpenTransaction;

// ReSharper disable once ClassNeverInstantiated.Global
public class AssignOpenTransactionCommand : IRequest
{
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public required Guid TransactionId { get; init; }

    public required Guid OtherAccountId { get; init; }
    // ReSharper restore UnusedAutoPropertyAccessor.Global
}