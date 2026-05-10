using Kaesseli.Features.AccountSuggestion;
using Shouldly;
using Xunit;

namespace Kaesseli.Test.Features.AccountSuggestion;

public class AccountSuggestionJobStatusTests
{
    [Fact]
    public void TryStart_FirstCallStarts_SecondCallRejected()
    {
        var status = new AccountSuggestionJobStatus();

        status.TryStart(Guid.NewGuid(), total: 5, DateTimeOffset.UtcNow).ShouldBeTrue();
        status.IsRunning.ShouldBeTrue();
        status.Total.ShouldBe(5);

        status.TryStart(Guid.NewGuid(), total: 99, DateTimeOffset.UtcNow).ShouldBeFalse();
        status.Total.ShouldBe(5);
    }

    [Fact]
    public void Increments_AreThreadSafe()
    {
        var status = new AccountSuggestionJobStatus();
        status.TryStart(Guid.NewGuid(), total: 0, DateTimeOffset.UtcNow);
        status.SetTotal(1000);

        Parallel.For(0, 1000, _ => status.IncrementProcessed());
        Parallel.For(0, 200, _ => status.IncrementFailed("oops"));

        status.Processed.ShouldBe(1000);
        status.Failed.ShouldBe(200);
        status.LastError.ShouldBe("oops");
    }

    [Fact]
    public void Finish_StopsRunningAndAllowsRestart()
    {
        var status = new AccountSuggestionJobStatus();
        status.TryStart(Guid.NewGuid(), total: 1, DateTimeOffset.UtcNow);
        status.Finish(DateTimeOffset.UtcNow);

        status.IsRunning.ShouldBeFalse();
        status.FinishedAt.ShouldNotBeNull();

        status.TryStart(Guid.NewGuid(), total: 3, DateTimeOffset.UtcNow).ShouldBeTrue();
        status.Total.ShouldBe(3);
    }
}
