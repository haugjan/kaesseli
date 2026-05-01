namespace Kaesseli.Features.Accounts;

public class AccountPlanEntry
{
    public string Number { get; set; } = null!;
    public string ShortName { get; set; } = null!;
    public string Name { get; set; } = null!;
    public AccountType Type { get; set; }
    public string Icon { get; set; } = null!;
    public string IconColor { get; set; } = null!;
}
