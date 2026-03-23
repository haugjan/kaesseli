using Kaesseli.Domain.Accounts;

namespace Kaesseli.Application.Accounts;

public static class AddAccount
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public record Query
    {
        public required string Name { get; init; }
        public required AccountType Type { get; init; }
        public required string Icon { get; init; }
        public required string IconColor { get; init; }
    }

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
                              account: new Account
                              {
                                  Type = request.Type,
                                  Name = request.Name,
                                  Icon = new AccountIcon(request.Icon, request.IconColor),
                                  Id = Guid.NewGuid()
                              },
                              cancellationToken);
            return account.Id;
        }
    }
}
