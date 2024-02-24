using MediatR;

namespace Kaesseli.Application.Integration;

// ReSharper disable once ClassNeverInstantiated.Global
public class ProcessCamtFileCommand : IRequest<Guid>
{
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public required string Content { get; init; }
    public required Guid AccountId { get; init; }
    // ReSharper restore UnusedAutoPropertyAccessor.Global
}