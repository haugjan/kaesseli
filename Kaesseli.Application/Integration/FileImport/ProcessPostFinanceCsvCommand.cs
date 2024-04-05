using MediatR;

namespace Kaesseli.Application.Integration.FileImport;

public class ProcessPostFinanceCsvCommand : IRequest<Guid>
{
    public required Stream Content { get; init; }
    public required Guid AccountId { get; init; }
}