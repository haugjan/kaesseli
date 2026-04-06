namespace Kaesseli.Features.Accounts;

public static class UpdateAccount
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public record Query(Guid Id, string Name, AccountType Type, string Icon, string IconColor);

    public interface IHandler
    {
        Task Handle(Query request, CancellationToken cancellationToken);
    }

    // ReSharper disable once UnusedType.Global
    public class Handler(IAccountRepository repo) : IHandler
    {
        public async Task Handle(Query request, CancellationToken cancellationToken)
        {
            var account = await repo.GetAccount(request.Id, cancellationToken);
            account.Update(request.Name, request.Type, new AccountIcon(request.Icon, request.IconColor));
            await repo.UpdateAccount(account, cancellationToken);
        }
    }
}
