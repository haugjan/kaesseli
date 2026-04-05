namespace Kaesseli.Features.Accounts;

public static class AddAccountingPeriod
{
    public record Query(DateOnly FromInclusive, DateOnly ToInclusive, string? Description);

    // ReSharper disable once UnusedType.Global
    public interface IHandler
    {
        Task<Guid> Handle(Query request, CancellationToken cancellationToken);
    }

    // ReSharper disable once UnusedType.Global
    public class Handler(IAccountRepository accountRepository) : IHandler
    {
        public async Task<Guid> Handle(Query request, CancellationToken cancellationToken)
        {
            var description = string.IsNullOrWhiteSpace(request.Description)
                ? $"{request.FromInclusive:d} - {request.ToInclusive:d}"
                : request.Description;
            var accountingPeriod = await accountRepository.AddAccountingPeriod(
                AccountingPeriod.Create(description, request.FromInclusive, request.ToInclusive),
                cancellationToken
            );
            return accountingPeriod.Id;
        }
    }
}
