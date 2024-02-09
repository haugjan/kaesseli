using Kaesseli.Application.Accounts;
using Kaesseli.Domain.Accounts;
using Kaesseli.Domain.Common;
using Moq;
using Xunit;

namespace Kaesseli.Application.Test.Accounts;

public class AddAccountCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldAddAccountSuccessfully()
    {
        // Arrange
        var mockRepo = new Mock<IAccountRepository>();
        const string name = "MyAccount";
        var command = new AddAccountCommand { Name = name, Type = AccountType.Expense};
        var cancellationToken = new CancellationToken();

        mockRepo.Setup(
                    repo => repo.AddAccount(
                        It.IsAny<Account>(),
                        cancellationToken))
                .ReturnsAsync((Account newAccount, CancellationToken _) => newAccount);

        var handler = new AddAccountCommandHandler(mockRepo.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        mockRepo.Verify(repo => repo.AddAccount(It.Is<Account>(acc => acc.Name == name && acc.Id == result), cancellationToken));
    }
}