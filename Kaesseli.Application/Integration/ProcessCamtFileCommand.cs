using MediatR;

namespace Kaesseli.Application.Integration;

public class ProcessCamtFileCommand : IRequest<IEnumerable<Guid>>
{
    public required string Content { get; init; }
    public required Guid AccountId { get; init; }
}