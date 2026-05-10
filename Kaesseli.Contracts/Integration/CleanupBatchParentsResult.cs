namespace Kaesseli.Contracts.Integration;

public record CleanupBatchParentsResult(
    int ParentsFound,
    int JournalEntriesAffected,
    bool DryRun,
    IReadOnlyList<BatchParentInfo> Parents);

public record BatchParentInfo(
    Guid Id,
    string Reference,
    string Description,
    decimal Amount,
    DateOnly ValueDate,
    int ChildCount,
    int JournalEntryCount);
