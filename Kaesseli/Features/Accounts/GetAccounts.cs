
namespace Kaesseli.Features.Accounts;

public static class GetAccounts
{
    public record Query(AccountType? AccountType = null);

    public record Result(Guid Id, string Name, AccountType TypeId, string Icon, string IconColor)
    {
        public string Type => TypeId.DisplayName();
    }

    public interface IHandler
    {
        Task<IEnumerable<Result>> Handle(Query request, CancellationToken cancellationToken);
    }

    // ReSharper disable once UnusedType.Global
    public class Handler(IAccountRepository repository) : IHandler
    {
        public async Task<IEnumerable<Result>> Handle(Query request, CancellationToken cancellationToken)
        {
            var accounts = request.AccountType is null
                               ? await repository.GetAccounts(cancellationToken)
                               : await repository.GetAccounts(request.AccountType.Value, cancellationToken);
            return accounts.Select(
                account => new Result(
                    Id: account.Id,
                    Name: account.Name,
                    TypeId: account.Type,
                    Icon: account.Icon.Name,
                    IconColor: account.Icon.Color));
        }
    }
}
