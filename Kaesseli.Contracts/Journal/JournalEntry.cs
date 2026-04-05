namespace Kaesseli.Contracts.Journal;

public record JournalEntry(Guid Id, Guid? DebitAccountId, Guid? CreditAccountId, decimal Amount, string Description, DateOnly ValueDate);
