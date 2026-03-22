namespace Kaesseli.Infrastructure.Integration;

public class TransactionStatistic
{
    public required Guid Id { get;init; }
    public required int TotalOpenTransaction { get; set; }
}
