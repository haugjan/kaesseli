namespace Kaesseli.Contracts.Features.Journal;

public static class GetJournalEntriesContract
{
    public record Result(Guid Id, Guid? DebitAccountId, Guid? CreditAccountId, decimal Amount, string Description, DateOnly ValueDate);
}
