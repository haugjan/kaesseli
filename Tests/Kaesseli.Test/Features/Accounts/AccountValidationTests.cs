using Kaesseli.Features.Accounts;
using Shouldly;
using Xunit;

namespace Kaesseli.Test.Features.Accounts;

public class AccountValidationTests
{
    private static readonly AccountIcon Icon = new("favorite", "blue");

    [Theory]
    [InlineData("100")] // too short
    [InlineData("10000")] // too long
    [InlineData("0999")] // first digit out of range
    [InlineData("5000")] // first digit out of range
    [InlineData("abcd")] // not numeric
    [InlineData("")] // empty
    public void Create_WithInvalidNumberFormat_Throws(string number) =>
        Should.Throw<InvalidAccountNumberException>(() =>
            Account.Create("Acc", AccountType.Asset, number, "asset", Icon)
        );

    [Theory]
    [InlineData(AccountType.Asset, "2000")]
    [InlineData(AccountType.Liability, "1000")]
    [InlineData(AccountType.Revenue, "4000")]
    [InlineData(AccountType.Expense, "3000")]
    public void Create_WithMismatchedNumberAndType_Throws(AccountType type, string number) =>
        Should.Throw<AccountNumberDoesNotMatchTypeException>(() =>
            Account.Create("Acc", type, number, "acc", Icon)
        );

    [Theory]
    [InlineData("A")] // too short
    [InlineData("ABC")] // uppercase
    [InlineData("with space")] // space
    [InlineData("under_score")] // underscore
    [InlineData("twenty-one-chars-XXXX")] // too long + uppercase
    [InlineData("")] // empty
    public void Create_WithInvalidShortName_Throws(string shortName) =>
        Should.Throw<InvalidAccountShortNameException>(() =>
            Account.Create("Acc", AccountType.Asset, "1000", shortName, Icon)
        );

    [Theory]
    [InlineData(AccountType.Asset, "1234")]
    [InlineData(AccountType.Liability, "2999")]
    [InlineData(AccountType.Revenue, "3500")]
    [InlineData(AccountType.Expense, "4001")]
    public void Create_WithValidInputs_Succeeds(AccountType type, string number)
    {
        var account = Account.Create("Acc", type, number, "valid-name", Icon);

        account.Number.ShouldBe(number);
        account.ShortName.ShouldBe("valid-name");
        account.Type.ShouldBe(type);
    }

    [Fact]
    public void Update_AppliesNewValuesAfterValidation()
    {
        var account = Account.Create("Old", AccountType.Asset, "1000", "old-name", Icon);

        account.Update("New", AccountType.Liability, "2500", "new-name", Icon);

        account.Name.ShouldBe("New");
        account.Number.ShouldBe("2500");
        account.ShortName.ShouldBe("new-name");
        account.Type.ShouldBe(AccountType.Liability);
    }
}
