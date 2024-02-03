using MediatR;

namespace Kaesseli.Application.Accounts;

public class AddAccountCommand : IRequest<Guid>
{
    public required string Name { get; init; }
}