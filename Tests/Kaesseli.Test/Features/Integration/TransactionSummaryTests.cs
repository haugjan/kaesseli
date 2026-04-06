using Kaesseli.Features.Accounts;
using Kaesseli.Features.Integration;
using Kaesseli.Test.Faker;
using Shouldly;
using Xunit;

namespace Kaesseli.Test.Features.Integration;

public class TransactionSummaryTests
{
    [Fact]
    public void Create_ValidInput_SetsProperties()
    {
        var account = new SmartFaker<Account>().Generate();
        var transactions = new SmartFaker<Transaction>().Generate(count: 3);

        var summary = TransactionSummary.Create(
            account, 1000m, 500m,
            new DateOnly(2026, 1, 1), new DateOnly(2026, 3, 31),
            "REF-2026-Q1", transactions);

        summary.Id.ShouldNotBe(Guid.Empty);
        summary.Account.ShouldBe(account);
        summary.BalanceBefore.ShouldBe(1000m);
        summary.BalanceAfter.ShouldBe(500m);
        summary.Reference.ShouldBe("REF-2026-Q1");
        summary.Transactions.Count().ShouldBe(3);
    }

    [Fact]
    public void Create_NullAccount_Throws()
    {
        Should.Throw<ArgumentNullException>(() =>
            TransactionSummary.Create(null!, 0m, 0m, default, default, "ref", []));
    }
}
