using Kaesseli.Domain.Accounts;
using MediatR;

namespace Kaesseli.Application.Accounts;

// ReSharper disable once UnusedType.Global
public class AddAccountCommandHandler(IAccountRepository repo) :
    IRequestHandler<AddAccountCommand, Guid>
{
    public async Task<Guid> Handle(AddAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await repo.AddAccount(
                          account: new Account
                          {
                              Type = request.Type,
                              Name = request.Name,
                              Icon = new AccountIcon(request.Icon, request.IconColor),
                              Id = Guid.NewGuid()
                          },
                          cancellationToken);
        return account.Id;
    }
}