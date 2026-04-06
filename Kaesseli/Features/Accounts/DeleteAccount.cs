namespace Kaesseli.Features.Accounts;

public static class DeleteAccount
{
    public record Query(Guid Id);

    public interface IHandler
    {
        Task Handle(Query request, CancellationToken cancellationToken);
    }

    // ReSharper disable once UnusedType.Global
    public class Handler(IAccountRepository repo) : IHandler
    {
        public async Task Handle(Query request, CancellationToken cancellationToken)
        {
            await repo.DeleteAccount(request.Id, cancellationToken);
        }
    }
}
