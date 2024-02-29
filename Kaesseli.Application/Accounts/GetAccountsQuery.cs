using Kaesseli.Domain.Accounts;
using MediatR;

namespace Kaesseli.Application.Accounts;

public class GetAccountsQuery : IRequest<IEnumerable<GetAccountsQueryResult>>
{
    public AccountType? AccountType { get; init; }
}