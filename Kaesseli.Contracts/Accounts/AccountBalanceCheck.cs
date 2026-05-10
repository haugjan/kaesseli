namespace Kaesseli.Contracts.Accounts;

public record AccountBalanceCheck(
    Guid AccountId,
    decimal CurrentBalance,
    decimal? LatestStatementBalance,
    DateOnly? LatestStatementDate,
    decimal? Difference);
