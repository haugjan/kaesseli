using MediatR;

namespace Kaesseli.Application.Accounts;

// ReSharper disable once ClassNeverInstantiated.Global
public class AddAccountCommand : IRequest<Guid>
{
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public required string Name { get; init; }
}