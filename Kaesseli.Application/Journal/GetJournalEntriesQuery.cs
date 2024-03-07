using Kaesseli.Domain.Accounts;
using MediatR;

namespace Kaesseli.Application.Journal;

public class GetJournalEntriesQuery : IRequest<IEnumerable<GetJournalEntriesQueryResult>>
{
    public required Guid AccountingPeriodId { get; init; }
    public required Guid? AccountId { get; init; }
    public required AccountType? AccountType { get; init; }
}