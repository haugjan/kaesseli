using Kaesseli.Domain.Accounts;
using MediatR;

namespace Kaesseli.Application.Accounts;

// ReSharper disable once UnusedType.Global
public class GetAccountsQueryHandler(IAccountRepository repository)
    : IRequestHandler<GetAccountsQuery, IEnumerable<GetAccountsQueryResult>>
{
    public async Task<IEnumerable<GetAccountsQueryResult>> Handle(GetAccountsQuery request, CancellationToken cancellationToken)
    {
        var accounts = await repository.GetAccounts(cancellationToken);
        return accounts.Select(account => new GetAccountsQueryResult { Id = account.Id, Name = account.Name, Type = account.Type });
    }
}