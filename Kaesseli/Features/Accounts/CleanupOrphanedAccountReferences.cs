namespace Kaesseli.Features.Accounts;

public static class CleanupOrphanedAccountReferences
{
    public interface IHandler
    {
        Task<Contracts.Accounts.CleanupOrphansResult> Handle(CancellationToken cancellationToken);
    }

    // ReSharper disable once UnusedType.Global
    public class Handler(IAccountRepository repo) : IHandler
    {
        public async Task<Contracts.Accounts.CleanupOrphansResult> Handle(
            CancellationToken cancellationToken
        )
        {
            var counts = await repo.CleanupOrphanedAccountReferences(cancellationToken);
            return new Contracts.Accounts.CleanupOrphansResult(
                JournalEntriesDeleted: counts.JournalEntriesDeleted,
                BudgetEntriesDeleted: counts.BudgetEntriesDeleted
            );
        }
    }
}
