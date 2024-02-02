using Kaesseli.Domain.Budget;
using Kaesseli.Domain.Common;
using MediatR;

namespace Kaesseli.Application.Budget;

public class AddAccountCommandHandler : IRequestHandler<AddAccountCommand, Guid>
{
    private readonly IBudgetRepository _repo;

    public AddAccountCommandHandler(IBudgetRepository repo)
    {
        _repo = repo;
    }

    public async Task<Guid> Handle(AddAccountCommand request, CancellationToken cancellationToken)
    {
        var account= await _repo.AddAccount(account: new Account
        {
            Name = request.Name,
            Id = Guid.NewGuid()
        }, cancellationToken);
        return account.Id;
    }
}