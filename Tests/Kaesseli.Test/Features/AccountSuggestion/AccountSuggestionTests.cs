using Kaesseli.Features.AccountSuggestion;
using Shouldly;
using Xunit;

namespace Kaesseli.Test.Features.AccountSuggestions;

public class AccountSuggestionTests
{
    [Fact]
    public void Create_AssignsFields()
    {
        var transactionId = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var generatedAt = DateTimeOffset.UtcNow;
        var item = AccountSuggestionItem.Create(accountId, confidence: 0.85, rank: 1);

        var suggestion = AccountSuggestion.Create(transactionId, generatedAt, [item]);

        suggestion.TransactionId.ShouldBe(transactionId);
        suggestion.GeneratedAt.ShouldBe(generatedAt);
        suggestion.Items.ShouldHaveSingleItem();
        suggestion.Items[0].AccountId.ShouldBe(accountId);
        suggestion.Items[0].Confidence.ShouldBe(0.85);
        suggestion.Items[0].Rank.ShouldBe(1);
        suggestion.Error.ShouldBeNull();
    }

    [Fact]
    public void Create_WithEmptyTransactionId_Throws()
    {
        Should.Throw<ArgumentException>(() =>
            AccountSuggestion.Create(Guid.Empty, DateTimeOffset.UtcNow, [])
        );
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.1)]
    public void Item_Create_RejectsConfidenceOutOfRange(double confidence)
    {
        Should.Throw<ArgumentOutOfRangeException>(() =>
            AccountSuggestionItem.Create(Guid.NewGuid(), confidence, rank: 1)
        );
    }

    [Fact]
    public void Item_Create_RejectsRankBelowOne()
    {
        Should.Throw<ArgumentOutOfRangeException>(() =>
            AccountSuggestionItem.Create(Guid.NewGuid(), confidence: 0.5, rank: 0)
        );
    }

    [Fact]
    public void Item_Create_RejectsEmptyAccountId()
    {
        Should.Throw<ArgumentException>(() =>
            AccountSuggestionItem.Create(Guid.Empty, confidence: 0.5, rank: 1)
        );
    }
}
