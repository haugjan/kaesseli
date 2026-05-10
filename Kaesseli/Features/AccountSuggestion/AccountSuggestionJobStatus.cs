namespace Kaesseli.Features.AccountSuggestion;

public class AccountSuggestionJobStatus
{
    private readonly Lock _lock = new();

    public bool IsRunning { get; private set; }
    public Guid? RunId { get; private set; }
    public DateTimeOffset? StartedAt { get; private set; }
    public DateTimeOffset? FinishedAt { get; private set; }
    public int Total { get; private set; }
    public int Processed { get; private set; }
    public int Failed { get; private set; }
    public string? LastError { get; private set; }

    internal bool TryStart(Guid runId, int total, DateTimeOffset now)
    {
        lock (_lock)
        {
            if (IsRunning) return false;
            IsRunning = true;
            RunId = runId;
            StartedAt = now;
            FinishedAt = null;
            Total = total;
            Processed = 0;
            Failed = 0;
            LastError = null;
            return true;
        }
    }

    internal void SetTotal(int total)
    {
        lock (_lock) Total = total;
    }

    internal void IncrementProcessed()
    {
        lock (_lock) Processed++;
    }

    internal void IncrementFailed(string? error)
    {
        lock (_lock)
        {
            Failed++;
            LastError = error;
        }
    }

    internal void Finish(DateTimeOffset now)
    {
        lock (_lock)
        {
            IsRunning = false;
            FinishedAt = now;
        }
    }
}
