namespace Kaesseli.Features.Journal;

public static class DeleteJournalEntry
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public record Query(Guid Id);

    public interface IHandler
    {
        Task Handle(Query request, CancellationToken cancellationToken);
    }

    // ReSharper disable once UnusedType.Global
    public class Handler(IJournalRepository journalRepository) : IHandler
    {
        public Task Handle(Query request, CancellationToken cancellationToken) =>
            journalRepository.DeleteJournalEntry(request.Id, cancellationToken);
    }
}
