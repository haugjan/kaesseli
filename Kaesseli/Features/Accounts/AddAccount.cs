namespace Kaesseli.Features.Accounts;

public static class AddAccount
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public record Query(
        string Name,
        AccountType Type,
        string Number,
        string ShortName,
        string Icon,
        string IconColor
    );

    public interface IHandler
    {
        Task<Guid> Handle(Query request, CancellationToken cancellationToken);
    }

    // ReSharper disable once UnusedType.Global
    public class Handler(IAccountRepository repo) : IHandler
    {
        public async Task<Guid> Handle(Query request, CancellationToken cancellationToken)
        {
            if (
                await repo.AccountNumberExists(
                    request.Number,
                    excludeAccountId: null,
                    cancellationToken
                )
            )
                throw new DuplicateAccountNumberException(request.Number);
            if (
                await repo.AccountShortNameExists(
                    request.ShortName,
                    excludeAccountId: null,
                    cancellationToken
                )
            )
                throw new DuplicateAccountShortNameException(request.ShortName);

            var account = await repo.AddAccount(
                Account.Create(
                    request.Name,
                    request.Type,
                    request.Number,
                    request.ShortName,
                    new AccountIcon(request.Icon, request.IconColor)
                ),
                cancellationToken
            );
            return account.Id;
        }
    }
}
