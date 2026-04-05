
namespace Kaesseli.Features.Accounts;

public static class AddAccount
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public record Query(string Name, AccountType Type, string Icon, string IconColor);

    public interface IHandler
    {
        Task<Guid> Handle(Query request, CancellationToken cancellationToken);
    }

    // ReSharper disable once UnusedType.Global
    public class Handler(IAccountRepository repo) : IHandler
    {
        public async Task<Guid> Handle(Query request, CancellationToken cancellationToken)
        {
            var account = await repo.AddAccount(
                              Account.Create(request.Name, request.Type, new AccountIcon(request.Icon, request.IconColor)),
                              cancellationToken);
            return account.Id;
        }
    }
}
