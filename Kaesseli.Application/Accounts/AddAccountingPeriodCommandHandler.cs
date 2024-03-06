using Kaesseli.Domain.Accounts;
using MediatR;

namespace Kaesseli.Application.Accounts;

// ReSharper disable once UnusedType.Global
public class AddAccountingPeriodCommandHandler : IRequestHandler<AddAccountingPeriodCommand, Guid>
{
    private readonly IAccountRepository _accountRepository;

    public AddAccountingPeriodCommandHandler(IAccountRepository accountRepository) =>
        _accountRepository = accountRepository;

    public async Task<Guid> Handle(AddAccountingPeriodCommand request, CancellationToken cancellationToken)
    {
        var accountingPeriod = await _accountRepository.AddAccountingPeriod(
                                   accountingPeriod: new AccountingPeriod
                                   {
                                       Id = Guid.NewGuid(),
                                       Description = string.IsNullOrWhiteSpace(request.Description)
                                                         ? $"{request.FromInclusive:d} - {request.ToInclusive:id}"
                                                         : request.Description,
                                       FromInclusive = request.FromInclusive,
                                       ToInclusive = request.ToInclusive
                                   },
                                   cancellationToken);
        return accountingPeriod.Id;
    }
}