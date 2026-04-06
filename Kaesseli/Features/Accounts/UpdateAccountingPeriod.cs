namespace Kaesseli.Features.Accounts;

public static class UpdateAccountingPeriod
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public record Query(Guid Id, string Description, DateOnly FromInclusive, DateOnly ToInclusive);

    public interface IHandler
    {
        Task Handle(Query request, CancellationToken cancellationToken);
    }

    // ReSharper disable once UnusedType.Global
    public class Handler(IAccountRepository accountRepository) : IHandler
    {
        public async Task Handle(Query request, CancellationToken cancellationToken)
        {
            var period = await accountRepository.GetAccountingPeriod(request.Id, cancellationToken);
            period.Update(request.Description, request.FromInclusive, request.ToInclusive);
            await accountRepository.UpdateAccountingPeriod(period, cancellationToken);
        }
    }
}
