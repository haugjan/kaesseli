
namespace Kaesseli.Features.Accounts;

public static class AddAccountingPeriod
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public record Query(DateOnly FromInclusive, DateOnly ToInclusive, string? Description);

    public interface IHandler
    {
        Task<Guid> Handle(Query request, CancellationToken cancellationToken);
    }

    // ReSharper disable once UnusedType.Global
    public class Handler(IAccountRepository accountRepository) : IHandler
    {
        public async Task<Guid> Handle(Query request, CancellationToken cancellationToken)
        {
            var accountingPeriod = await accountRepository.AddAccountingPeriod(
                                       accountingPeriod: new AccountingPeriod
                                       {
                                           Id = Guid.NewGuid(),
                                           Description = string.IsNullOrWhiteSpace(request.Description)
                                                             ? $"{request.FromInclusive:d} - {request.ToInclusive:d}"
                                                             : request.Description,
                                           FromInclusive = request.FromInclusive,
                                           ToInclusive = request.ToInclusive
                                       },
                                       cancellationToken);
            return accountingPeriod.Id;
        }
    }
}
