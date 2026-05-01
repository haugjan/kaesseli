namespace Kaesseli.Contracts.Accounts;

public record CleanupOrphansResult(int JournalEntriesDeleted, int BudgetEntriesDeleted);
