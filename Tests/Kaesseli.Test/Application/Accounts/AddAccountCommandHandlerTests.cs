using Kaesseli.Application.Accounts;
using Kaesseli.Domain.Accounts;
using Moq;
using Xunit;

namespace Kaesseli.Test.Application.Accounts;

public class AddAccountCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldAddAccountSuccessfully()
    {
        // Arrange
        var mockRepo = new Mock<IAccountRepository>();
        const string name = "MyAccount";
        var command = new AddAccount.Query(Name: name, Type: AccountType.Expense, Icon: "favorite", IconColor: "blue");
        var cancellationToken = new CancellationToken();

        mockRepo
            .Setup(repo => repo.AddAccount(It.IsAny<Account>(), cancellationToken))
            .ReturnsAsync((Account newAccount, CancellationToken _) => newAccount);

        var handler = new AddAccount.Handler(mockRepo.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        mockRepo.Verify(repo =>
            repo.AddAccount(
                It.Is<Account>(acc => acc.Name == name && acc.Id == result),
                cancellationToken
            )
        );
    }
}
