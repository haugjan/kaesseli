namespace Kaesseli.Contracts.Accounts;

public record Account(Guid Id, string Name, AccountType TypeId, string Icon, string IconColor);
