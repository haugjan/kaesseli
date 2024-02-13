using Kaesseli.Domain.Accounts;
using MediatR;

namespace Kaesseli.Application.Journal;

// ReSharper disable once ClassNeverInstantiated.Global
public class AddJournalEntryCommand : IRequest<Guid>
{
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public required decimal Amount { get; init; }
    public required string Description { get; init; }
    public required DateOnly? ValueDate { get; init; }
    public required Account CreditAccountId { get; init; }
    public required Account DebitAccountId { get; init; }
    // ReSharper restore UnusedAutoPropertyAccessor.Global

}