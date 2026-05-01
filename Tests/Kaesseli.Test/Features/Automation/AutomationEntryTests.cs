using Kaesseli.Features.Automation;
using Shouldly;
using Xunit;

namespace Kaesseli.Test.Features.Automation;

public class AutomationEntryTests
{
    [Fact]
    public void Create_ValidInput_SetsProperties()
    {
        var part = AutomationEntryPart.Create("groceries", 0.5m);
        var entry = AutomationEntry.Create("MIGROS*", [part]);

        entry.Id.ShouldNotBe(Guid.Empty);
        entry.AutomationText.ShouldBe("MIGROS*");
        entry.Parts.ShouldContain(part);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_EmptyText_Throws(string? text)
    {
        Should.Throw<ArgumentException>(() => AutomationEntry.Create(text!, []));
    }

    [Fact]
    public void Create_NullParts_Throws()
    {
        Should.Throw<ArgumentNullException>(() => AutomationEntry.Create("test", null!));
    }
}

public class AutomationEntryPartTests
{
    [Fact]
    public void Create_ValidInput_SetsProperties()
    {
        var part = AutomationEntryPart.Create("groceries", 0.75m);

        part.Id.ShouldNotBe(Guid.Empty);
        part.AccountShortName.ShouldBe("groceries");
        part.AmountProportion.ShouldBe(0.75m);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_EmptyShortName_Throws(string? shortName)
    {
        Should.Throw<ArgumentException>(() => AutomationEntryPart.Create(shortName!, 1m));
    }
}
