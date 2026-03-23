using Kaesseli.Domain.Accounts;

namespace Kaesseli.Application.Accounts;

public static class AddAccountingPeriod
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public record Query
    {
        // ReSharper disable UnusedAutoPropertyAccessor.Global
        public required DateOnly FromInclusive { get; init; }
        public required DateOnly ToInclusive { get; init; }
        public required string? Description { get; init; }
        // ReSharper restore UnusedAutoPropertyAccessor.Global
    }

    public interface IHandler
    {
        Task<Guid> Handle(Query request, CancellationToken cancellationToken);
    }

    // ReSharper disable once UnusedType.Global
    public class Handler : IHandler
    {
        private readonly IAccountRepository _accountRepository;

        public Handler(IAccountRepository accountRepository) =>
            _accountRepository = accountRepository;

        public async Task<Guid> Handle(Query request, CancellationToken cancellationToken)
        {
            var accountingPeriod = await _accountRepository.AddAccountingPeriod(
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
