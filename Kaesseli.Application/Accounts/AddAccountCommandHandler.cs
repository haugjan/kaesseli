using Kaesseli.Domain.Accounts;
using Kaesseli.Domain.Common;
using MediatR;

namespace Kaesseli.Application.Accounts;

// ReSharper disable once UnusedType.Global
public class AddAccountCommandHandler(IAccountRepository repo) : 
    IRequestHandler<AddAccountCommand, Guid>
{
    public async Task<Guid> Handle(AddAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await repo.AddAccount(account: new Account { Name = request.Name, Id = Guid.NewGuid() }, cancellationToken);
        return account.Id;
    }
}