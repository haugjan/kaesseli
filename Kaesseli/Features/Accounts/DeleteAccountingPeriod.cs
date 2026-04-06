namespace Kaesseli.Features.Accounts;

public static class DeleteAccountingPeriod
{
    public record Query(Guid Id);

    public interface IHandler
    {
        Task Handle(Query request, CancellationToken cancellationToken);
    }

    // ReSharper disable once UnusedType.Global
    public class Handler(IAccountRepository accountRepository) : IHandler
    {
        public async Task Handle(Query request, CancellationToken cancellationToken)
        {
            await accountRepository.DeleteAccountingPeriod(request.Id, cancellationToken);
        }
    }
}
