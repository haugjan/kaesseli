using Kaesseli.Features.Accounts;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Kaesseli.Test.Features.Accounts;

public class AddAccountDuplicateTests
{
    [Fact]
    public async Task Handle_WithDuplicateNumber_Throws()
    {
        var repo = Substitute.For<IAccountRepository>();
        repo.AccountNumberExists("1000", null, Arg.Any<CancellationToken>()).Returns(true);

        var handler = new AddAccount.Handler(repo);

        await Should.ThrowAsync<DuplicateAccountNumberException>(() =>
            handler.Handle(
                new AddAccount.Query(
                    "Bank",
                    AccountType.Asset,
                    "1000",
                    "bank",
                    "AccountBalance",
                    "blue"
                ),
                CancellationToken.None
            )
        );

        await repo.DidNotReceiveWithAnyArgs()
            .AddAccount(Arg.Any<Account>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithDuplicateShortName_Throws()
    {
        var repo = Substitute.For<IAccountRepository>();
        repo.AccountNumberExists("1000", null, Arg.Any<CancellationToken>()).Returns(false);
        repo.AccountShortNameExists("bank", null, Arg.Any<CancellationToken>()).Returns(true);

        var handler = new AddAccount.Handler(repo);

        await Should.ThrowAsync<DuplicateAccountShortNameException>(() =>
            handler.Handle(
                new AddAccount.Query(
                    "Bank",
                    AccountType.Asset,
                    "1000",
                    "bank",
                    "AccountBalance",
                    "blue"
                ),
                CancellationToken.None
            )
        );

        await repo.DidNotReceiveWithAnyArgs()
            .AddAccount(Arg.Any<Account>(), Arg.Any<CancellationToken>());
    }
}
