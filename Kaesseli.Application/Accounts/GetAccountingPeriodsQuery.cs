using MediatR;

namespace Kaesseli.Application.Accounts;

public class GetAccountingPeriodsQuery : IRequest<IEnumerable<GetAccountingPeriodsQueryResult>>;