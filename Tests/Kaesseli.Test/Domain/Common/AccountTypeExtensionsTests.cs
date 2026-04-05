using Kaesseli.Features.Accounts;
using Shouldly;
using Xunit;

namespace Kaesseli.Test.Domain.Common;

public class AccountTypeExtensionsTests
{
    [Theory]
    // ReSharper disable StringLiteralTypo
    [InlineData(AccountType.Asset, "Aktiv")]
    [InlineData(AccountType.Liability, "Passiv")]
    [InlineData(AccountType.Expense, "Ausgaben")]
    // ReSharper restore StringLiteralTypo
    public void AccountTypeToString_ShouldReturnExpectedString(
        AccountType accountType,
        string expectedString
    )
    {
        // Act
        var result = accountType.DisplayName();

        // Assert
        result.ShouldBe(expectedString);
    }

    [Fact]
    public void AccountTypeToString_ShouldThrowArgumentOutOfRangeExceptionForUndefinedValue()
    {
        // Arrange
        const AccountType accountType = (AccountType)(-1);
        var toDisplayName = () => accountType.DisplayName();

        // Act & Assert
        Should.Throw<ArgumentOutOfRangeException>(toDisplayName);
    }
}
