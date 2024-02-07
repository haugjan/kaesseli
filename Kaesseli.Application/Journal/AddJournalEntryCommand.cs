using MediatR;

namespace Kaesseli.Application.Journal;

// ReSharper disable once ClassNeverInstantiated.Global
public class AddJournalEntryCommand : IRequest<Guid>
{
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public required decimal Amount { get; init; }
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public required string Description { get; init; }
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public required DateOnly? ValueDate { get; init; }
}