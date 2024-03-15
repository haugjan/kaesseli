using MediatR;

namespace Kaesseli.Application.Integration.FileImport;

// ReSharper disable once ClassNeverInstantiated.Global
public class ProcessCamtFileCommand : IRequest<Guid>
{
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public required Stream Content { get; init; }

    public required Guid AccountId { get; init; }
    // ReSharper restore UnusedAutoPropertyAccessor.Global
}