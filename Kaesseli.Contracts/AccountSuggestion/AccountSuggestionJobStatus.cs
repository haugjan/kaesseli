namespace Kaesseli.Contracts.AccountSuggestion;

public record AccountSuggestionJobStatus(
    bool IsRunning,
    bool? Started,
    Guid? RunId,
    DateTimeOffset? StartedAt,
    DateTimeOffset? FinishedAt,
    int Total,
    int Processed,
    int Failed,
    string? LastError
);
