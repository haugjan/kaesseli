namespace Kaesseli.Contracts.Features.Accounts;

public static class GetAccountingPeriodsContract
{
    public record Result(Guid Id, string Description, DateOnly FromInclusive, DateOnly ToInclusive);
}
