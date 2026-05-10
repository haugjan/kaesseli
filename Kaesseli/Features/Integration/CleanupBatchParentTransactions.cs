namespace Kaesseli.Features.Integration;

public static class CleanupBatchParentTransactions
{
    public record Query(bool Execute);

    public interface IHandler
    {
        Task<Contracts.Integration.CleanupBatchParentsResult> Handle(
            Query request,
            CancellationToken cancellationToken
        );
    }

    // ReSharper disable once UnusedType.Global
    public class Handler(ITransactionRepository repo) : IHandler
    {
        public async Task<Contracts.Integration.CleanupBatchParentsResult> Handle(
            Query request,
            CancellationToken cancellationToken
        )
        {
            var outcome = await repo.CleanupBatchParentTransactions(
                dryRun: !request.Execute,
                cancellationToken
            );
            return new Contracts.Integration.CleanupBatchParentsResult(
                ParentsFound: outcome.Parents.Count,
                JournalEntriesAffected: outcome.Parents.Sum(p => p.JournalEntryCount),
                DryRun: outcome.DryRun,
                Parents: outcome
                    .Parents.Select(p => new Contracts.Integration.BatchParentInfo(
                        Id: p.Parent.Id,
                        Reference: p.Parent.Reference,
                        Description: p.Parent.Description,
                        Amount: p.Parent.Amount,
                        ValueDate: p.Parent.ValueDate,
                        ChildCount: p.ChildCount,
                        JournalEntryCount: p.JournalEntryCount
                    ))
                    .ToList()
            );
        }
    }
}
