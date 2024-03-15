using Kaesseli.Domain.Integration;
using MediatR;

namespace Kaesseli.Application.Integration.FileImport;

public class ProcessFileCommand : IRequest<Guid>
{
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public required FileType FileType { get; init; }
    public required Stream Content { get; init; }

    public required Guid AccountId { get; init; }

    public required Guid AccountingPeriodId { get; init; }
    // ReSharper restore UnusedAutoPropertyAccessor.Global
}