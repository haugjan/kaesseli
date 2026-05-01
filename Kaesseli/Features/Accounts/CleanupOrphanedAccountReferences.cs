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
        public Task<Contracts.Accounts.CleanupOrphansResult> Handle(
            CancellationToken cancellationToken
        ) => repo.CleanupOrphanedAccountReferences(cancellationToken);
    }
}
