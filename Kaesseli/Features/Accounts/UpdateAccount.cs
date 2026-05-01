namespace Kaesseli.Features.Accounts;

public static class UpdateAccount
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public record Query(
        Guid Id,
        string Name,
        AccountType Type,
        string Number,
        string ShortName,
        string Icon,
        string IconColor
    );

    public interface IHandler
    {
        Task Handle(Query request, CancellationToken cancellationToken);
    }

    // ReSharper disable once UnusedType.Global
    public class Handler(IAccountRepository repo) : IHandler
    {
        public async Task Handle(Query request, CancellationToken cancellationToken)
        {
            if (
                await repo.AccountNumberExists(
                    request.Number,
                    excludeAccountId: request.Id,
                    cancellationToken
                )
            )
                throw new DuplicateAccountNumberException(request.Number);
            if (
                await repo.AccountShortNameExists(
                    request.ShortName,
                    excludeAccountId: request.Id,
                    cancellationToken
                )
            )
                throw new DuplicateAccountShortNameException(request.ShortName);

            var account = await repo.GetAccount(request.Id, cancellationToken);
            account.Update(
                request.Name,
                request.Type,
                request.Number,
                request.ShortName,
                new AccountIcon(request.Icon, request.IconColor)
            );
            await repo.UpdateAccount(account, cancellationToken);
        }
    }
}
