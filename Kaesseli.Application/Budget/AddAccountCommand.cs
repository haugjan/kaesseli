using MediatR;

namespace Kaesseli.Application.Budget;

public class AddAccountCommand : IRequest<Guid>
{
    public required string Name { get; init; }
}