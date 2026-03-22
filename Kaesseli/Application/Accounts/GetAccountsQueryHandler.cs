using Kaesseli.Domain.Accounts;

namespace Kaesseli.Application.Accounts;

public interface IGetAccountsQueryHandler
{
    Task<IEnumerable<GetAccountsQueryResult>> Handle(GetAccountsQuery request, CancellationToken cancellationToken);
}

// ReSharper disable once UnusedType.Global
public class GetAccountsQueryHandler(IAccountRepository repository) : IGetAccountsQueryHandler
{
    public async Task<IEnumerable<GetAccountsQueryResult>> Handle(GetAccountsQuery request, CancellationToken cancellationToken)
    {
        var accounts = request.AccountType is null
                           ? await repository.GetAccounts(cancellationToken)
                           : await repository.GetAccounts(request.AccountType.Value, cancellationToken);
        return accounts.Select(
            account => new GetAccountsQueryResult
            {
                Id = account.Id,
                Name = account.Name,
                TypeId = account.Type,
                Icon = account.Icon.Name,
                IconColor = account.Icon.Color
            });
    }
}
