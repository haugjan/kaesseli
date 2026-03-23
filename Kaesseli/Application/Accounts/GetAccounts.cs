using Kaesseli.Domain.Accounts;

namespace Kaesseli.Application.Accounts;

public static class GetAccounts
{
    public record Query
    {
        public AccountType? AccountType { get; init; }
    }

    public class Result
    {
        // ReSharper disable UnusedAutoPropertyAccessor.Global
        public required Guid Id { get; init; }
        public required string Name { get; init; }
        public string Type => TypeId.DisplayName();
        public required AccountType TypeId { get; init; }
        public required string Icon { get; init; }
        public required string IconColor { get; init; }
        // ReSharper restore UnusedAutoPropertyAccessor.Global
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
                account => new Result
                {
                    Id = account.Id,
                    Name = account.Name,
                    TypeId = account.Type,
                    Icon = account.Icon.Name,
                    IconColor = account.Icon.Color
                });
        }
    }
}
