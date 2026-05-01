namespace Kaesseli.Contracts.Accounts;

public record Account(
    Guid Id,
    string Name,
    AccountType TypeId,
    string Number,
    string ShortName,
    string Icon,
    string IconColor
);
