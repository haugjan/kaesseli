namespace Kaesseli.Infrastructure.Journal;

internal class WrongAmountException : Exception
{
    private WrongAmountException(decimal transactionAmount, decimal entriesAmount) : base(
        message: $"Sum of entries amount {entriesAmount} do not match with transaction amount {transactionAmount}.")
    {
    }

    public static void ThrowIfAmountNotMatch(decimal transactionAmount, decimal entriesAmount)
    {
        if (transactionAmount != entriesAmount) throw new WrongAmountException(transactionAmount, entriesAmount);
    }
}