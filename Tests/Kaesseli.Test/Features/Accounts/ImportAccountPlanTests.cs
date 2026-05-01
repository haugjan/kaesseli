using Kaesseli.Features.Accounts;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Kaesseli.Test.Features.Accounts;

public class ImportAccountPlanTests
{
    private readonly IAccountRepository _repo = Substitute.For<IAccountRepository>();
    private readonly ImportAccountPlan.Handler _handler;

    public ImportAccountPlanTests() => _handler = new ImportAccountPlan.Handler(_repo);

    [Fact]
    public async Task Handle_EmptyYaml_ReturnsZeroCounts()
    {
        _repo.GetAccounts(Arg.Any<CancellationToken>()).Returns(Array.Empty<Account>());

        var result = await _handler.Handle(
            new ImportAccountPlan.Query("[]"),
            CancellationToken.None
        );

        result.Created.ShouldBe(0);
        result.Updated.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_NewAccount_IsCreated()
    {
        _repo.GetAccounts(Arg.Any<CancellationToken>()).Returns(Array.Empty<Account>());

        var yaml = """
            - number: '1000'
              shortName: bank
              name: Bankkonto
              type: Asset
              icon: AccountBalance
              iconColor: '#1976D2'
            """;

        var result = await _handler.Handle(
            new ImportAccountPlan.Query(yaml),
            CancellationToken.None
        );

        result.Created.ShouldBe(1);
        result.Updated.ShouldBe(0);
        await _repo
            .Received(1)
            .AddAccount(
                Arg.Is<Account>(a =>
                    a.ShortName == "bank" && a.Number == "1000" && a.Type == AccountType.Asset
                ),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task Handle_ExistingShortNameSameType_UpdatesAccount()
    {
        var existing = AccountFactory.Create(
            "Alt",
            AccountType.Asset,
            new AccountIcon("Old", "blue")
        );
        // override the auto-generated short name to match the YAML
        SetShortName(existing, "bank");
        _repo.GetAccounts(Arg.Any<CancellationToken>()).Returns(new[] { existing });

        var yaml = """
            - number: '1100'
              shortName: bank
              name: Bankkonto Neu
              type: Asset
              icon: AccountBalance
              iconColor: '#1976D2'
            """;

        var result = await _handler.Handle(
            new ImportAccountPlan.Query(yaml),
            CancellationToken.None
        );

        result.Updated.ShouldBe(1);
        existing.Name.ShouldBe("Bankkonto Neu");
        existing.Number.ShouldBe("1100");
        await _repo.Received(1).UpdateAccount(existing, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ExistingShortNameDifferentType_Throws()
    {
        var existing = AccountFactory.Create(
            "Bank",
            AccountType.Asset,
            new AccountIcon("X", "blue")
        );
        SetShortName(existing, "bank");
        _repo.GetAccounts(Arg.Any<CancellationToken>()).Returns(new[] { existing });

        var yaml = """
            - number: '4000'
              shortName: bank
              name: Bank Aufwand
              type: Expense
              icon: X
              iconColor: '#000000'
            """;

        var ex = await Should.ThrowAsync<AccountPlanImportException>(() =>
            _handler.Handle(new ImportAccountPlan.Query(yaml), CancellationToken.None)
        );
        ex.Message.ShouldContain("bank");
        ex.Message.ShouldContain("Asset");
    }

    [Fact]
    public async Task Handle_NumberConflictWithOtherAccount_Throws()
    {
        var holder = AccountFactory.Create(
            "HolderName",
            AccountType.Asset,
            new AccountIcon("X", "blue")
        );
        SetShortName(holder, "holder");
        SetNumber(holder, "1100");
        _repo.GetAccounts(Arg.Any<CancellationToken>()).Returns(new[] { holder });

        var yaml = """
            - number: '1100'
              shortName: bank
              name: Bankkonto
              type: Asset
              icon: X
              iconColor: '#000000'
            """;

        var ex = await Should.ThrowAsync<AccountPlanImportException>(() =>
            _handler.Handle(new ImportAccountPlan.Query(yaml), CancellationToken.None)
        );
        ex.Message.ShouldContain("1100");
        ex.Message.ShouldContain("holder");
    }

    [Fact]
    public async Task Handle_DuplicateShortNameWithinImport_Throws()
    {
        _repo.GetAccounts(Arg.Any<CancellationToken>()).Returns(Array.Empty<Account>());

        var yaml = """
            - number: '1000'
              shortName: bank
              name: A
              type: Asset
              icon: X
              iconColor: '#fff'
            - number: '1100'
              shortName: bank
              name: B
              type: Asset
              icon: X
              iconColor: '#fff'
            """;

        await Should.ThrowAsync<AccountPlanImportException>(() =>
            _handler.Handle(new ImportAccountPlan.Query(yaml), CancellationToken.None)
        );
    }

    [Fact]
    public async Task Handle_InvalidYaml_Throws()
    {
        _repo.GetAccounts(Arg.Any<CancellationToken>()).Returns(Array.Empty<Account>());

        await Should.ThrowAsync<AccountPlanImportException>(() =>
            _handler.Handle(
                new ImportAccountPlan.Query("this is: not [a list"),
                CancellationToken.None
            )
        );
    }

    private static void SetShortName(Account account, string value) =>
        typeof(Account).GetProperty(nameof(Account.ShortName))!.SetValue(account, value);

    private static void SetNumber(Account account, string value) =>
        typeof(Account).GetProperty(nameof(Account.Number))!.SetValue(account, value);
}
