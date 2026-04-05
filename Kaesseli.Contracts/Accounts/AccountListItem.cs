namespace Kaesseli.Contracts.Accounts;

public record AccountListItem(Guid Id, string Name, AccountType TypeId, string Icon, string IconColor);
