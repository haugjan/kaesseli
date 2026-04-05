namespace Kaesseli.Features.Integration;

public class TransactionStatistic
{
    private TransactionStatistic() { }

    public Guid Id { get; private init; }
    public int TotalOpenTransaction { get; private set; }

    public static TransactionStatistic Create(int totalOpenTransaction)
    {
        return new TransactionStatistic
        {
            Id = Guid.NewGuid(),
            TotalOpenTransaction = totalOpenTransaction,
        };
    }

    public void ChangeTotalBy(int amount)
    {
        TotalOpenTransaction += amount;
    }
}
