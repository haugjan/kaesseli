using Kaesseli.Domain.Accounts;

namespace Kaesseli.Application.Accounts;

public interface IAddAccountCommandHandler
{
    Task<Guid> Handle(AddAccountCommand request, CancellationToken cancellationToken);
}

// ReSharper disable once UnusedType.Global
public class AddAccountCommandHandler(IAccountRepository repo) : IAddAccountCommandHandler
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
