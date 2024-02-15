using Kaesseli.Domain.Accounts;
using MediatR;

namespace Kaesseli.Application.Journal;

public class GetJournalEntriesQuery : IRequest<IEnumerable<GetJournalEntriesQueryResult>>
{
    public  Guid? DebitAccountId { get; init; }
    public  Guid? CreditAccountId { get; init; }
    public  DateOnly? FromDate { get; init; }
    public  DateOnly? ToDate { get; init; }
    public  AccountType? AccountType { get; init; }
}