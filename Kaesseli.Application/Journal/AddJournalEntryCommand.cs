using MediatR;

namespace Kaesseli.Application.Journal;

// ReSharper disable once ClassNeverInstantiated.Global
public class AddJournalEntryCommand : IRequest<Guid>
{
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public required decimal Amount { get; init; }
    public required string Description { get; init; }
    public required DateOnly? ValueDate { get; init; }
    public required Guid CreditAccountId { get; init; }

    public required Guid DebitAccountId { get; init; }
    // ReSharper restore UnusedAutoPropertyAccessor.Global
}