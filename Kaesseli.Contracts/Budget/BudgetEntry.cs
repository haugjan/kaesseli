namespace Kaesseli.Contracts.Budget;

public record BudgetEntry(Guid Id, decimal Amount, string Description, Guid AccountId, Guid AccountingPeriodId);
