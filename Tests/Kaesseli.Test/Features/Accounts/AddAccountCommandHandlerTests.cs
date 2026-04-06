using Kaesseli.Features.Accounts;
using NSubstitute;
using Xunit;

namespace Kaesseli.Test.Features.Accounts;

public class AddAccountCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldAddAccountSuccessfully()
    {
        // Arrange
        var mockRepo = Substitute.For<IAccountRepository>();
        const string name = "MyAccount";
        var command = new AddAccount.Query(Name: name, Type: AccountType.Expense, Icon: "favorite", IconColor: "blue");
        var cancellationToken = new CancellationToken();

        mockRepo
            .AddAccount(Arg.Any<Account>(), cancellationToken)
            .Returns(callInfo => callInfo.ArgAt<Account>(0));

        var handler = new AddAccount.Handler(mockRepo);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        await mockRepo.Received()
            .AddAccount(
                Arg.Is<Account>(acc => acc.Name == name && acc.Id == result),
                cancellationToken
            );
    }
}
