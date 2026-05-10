using Kaesseli.Features.AccountSuggestion;
using Kaesseli.Features.Integration.NextOpenTransaction;
using Kaesseli.Features.Accounts;
using Kaesseli.Features.Integration;
using Kaesseli.Test.Faker;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Kaesseli.Test.Features.Integration;

public class GetNextOpenTransactionQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsCorrectResult_WhenTransactionExists()
    {
        // Arrange
        var mockTransactionRepository = Substitute.For<ITransactionRepository>();
        var mockAccountRepository = Substitute.For<IAccountRepository>();
        var mockSuggestionRepository = Substitute.For<IAccountSuggestionRepository>();
        var handler = new GetNextOpenTransaction.Handler(
            mockTransactionRepository,
            mockAccountRepository,
            mockSuggestionRepository
        );

        var expectedTransaction = new SmartFaker<Transaction>().Generate();

        var accounts = new SmartFaker<Account>().Generate(count: 5);

        mockTransactionRepository
            .GetNextOpenTransaction(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(expectedTransaction);
        mockAccountRepository
            .GetAccounts(Arg.Any<CancellationToken>())
            .Returns(accounts);
        mockSuggestionRepository
            .GetByTransactionId(expectedTransaction.Id, Arg.Any<CancellationToken>())
            .Returns((AccountSuggestion?)null);

        var request = new GetNextOpenTransaction.Query(1);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result!.Id.ShouldBe(expectedTransaction.Id);
        result.Amount.ShouldBe(expectedTransaction.Amount);
        result.Description.ShouldBe(expectedTransaction.Description);
        result.ValueDate.ShouldBe(expectedTransaction.ValueDate);
        Assert.Equal(expectedTransaction.TransactionSummary?.Account.Name, result!.AccountName);

        Assert.Equal(accounts.Count, actual: result.SuggestedAccounts.Count());
        result.AiSuggestions.ShouldBeNull();
    }

    [Fact]
    public async Task Handle_ProjectsAiSuggestions_WhenAvailable()
    {
        var mockTransactionRepository = Substitute.For<ITransactionRepository>();
        var mockAccountRepository = Substitute.For<IAccountRepository>();
        var mockSuggestionRepository = Substitute.For<IAccountSuggestionRepository>();
        var handler = new GetNextOpenTransaction.Handler(
            mockTransactionRepository,
            mockAccountRepository,
            mockSuggestionRepository
        );

        var transaction = new SmartFaker<Transaction>().Generate();
        var accounts = new SmartFaker<Account>().Generate(count: 3);
        var firstAccountId = accounts[0].Id;
        var secondAccountId = accounts[1].Id;
        var suggestion = AccountSuggestion.Create(
            transaction.Id,
            DateTimeOffset.UtcNow,
            [
                AccountSuggestionItem.Create(secondAccountId, confidence: 0.7, rank: 2),
                AccountSuggestionItem.Create(firstAccountId, confidence: 0.9, rank: 1),
            ]
        );

        mockTransactionRepository
            .GetNextOpenTransaction(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(transaction);
        mockAccountRepository.GetAccounts(Arg.Any<CancellationToken>()).Returns(accounts);
        mockSuggestionRepository
            .GetByTransactionId(transaction.Id, Arg.Any<CancellationToken>())
            .Returns(suggestion);

        var result = await handler.Handle(new GetNextOpenTransaction.Query(0), CancellationToken.None);

        result.ShouldNotBeNull();
        result!.AiSuggestions.ShouldNotBeNull();
        result.AiSuggestions!.Count.ShouldBe(2);
        result.AiSuggestions[0].AccountId.ShouldBe(firstAccountId);
        result.AiSuggestions[0].Rank.ShouldBe(1);
        result.AiSuggestions[0].Confidence.ShouldBe(0.9);
        result.AiSuggestions[1].AccountId.ShouldBe(secondAccountId);
    }

    [Fact]
    public async Task Handle_ReturnsNull_WhenTransactionDoesNotExist()
    {
        // Arrange
        var mockTransactionRepository = Substitute.For<ITransactionRepository>();
        var mockAccountRepository = Substitute.For<IAccountRepository>();
        var mockSuggestionRepository = Substitute.For<IAccountSuggestionRepository>();
        var handler = new GetNextOpenTransaction.Handler(
            mockTransactionRepository,
            mockAccountRepository,
            mockSuggestionRepository
        );

        mockTransactionRepository
            .GetNextOpenTransaction(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((Transaction?)null);

        var request = new GetNextOpenTransaction.Query(1);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        result.ShouldBeNull();
    }
}
