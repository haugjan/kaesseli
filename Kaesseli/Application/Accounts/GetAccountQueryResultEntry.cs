namespace Kaesseli.Application.Accounts;

public class GetAccountQueryResultEntry
{
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public required Guid Id { get; init;}
    public required DateOnly ValueDate { get; init;}
    public required string Description { get; init;}
    public required decimal Amount { get; init;}
    public required AmountType AmountType { get; init;}
    public required string? OtherAccount { get; init;}
    public required Guid? OtherAccountId { get; init;}
    // ReSharper restore UnusedAutoPropertyAccessor.Global
}