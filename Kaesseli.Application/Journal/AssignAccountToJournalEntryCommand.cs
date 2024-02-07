using MediatR;

namespace Kaesseli.Application.Journal;

// ReSharper disable once ClassNeverInstantiated.Global
public class AssignAccountToJournalEntryCommand : IRequest
{
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public required Guid JournalEntryId { get; init; }
    public required Guid AccountId { get; init; }
    // ReSharper restore UnusedAutoPropertyAccessor.Global
}