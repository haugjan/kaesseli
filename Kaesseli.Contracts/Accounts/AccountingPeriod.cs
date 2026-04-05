namespace Kaesseli.Contracts.Accounts;

public record AccountingPeriod(Guid Id, string Description, DateOnly FromInclusive, DateOnly ToInclusive);
