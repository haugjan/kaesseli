using MediatR;

namespace Kaesseli.Application.Accounts;

public class GetAccountsQuery : IRequest<IEnumerable<GetAccountsQueryResult>>;