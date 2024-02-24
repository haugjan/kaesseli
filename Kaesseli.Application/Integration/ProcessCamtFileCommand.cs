using MediatR;

namespace Kaesseli.Application.Integration;

public class ProcessCamtFileCommand : IRequest<Guid>
{
    public required string Content { get; init; }
    public required Guid AccountId { get; init; }
}