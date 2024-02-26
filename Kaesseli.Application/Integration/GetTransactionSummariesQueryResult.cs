using Kaesseli.Domain.Integration;

namespace Kaesseli.Application.Integration;

public class GetTransactionSummariesQueryResult
{
    
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public required Guid Id { get; init; }
    public required string AccountName { get; init; }
    public required DateOnly ValueDateFrom { get; init; }
    public required DateOnly ValueDateTo { get; init; }
    public required decimal BalanceBefore { get; init; }
    public required decimal BalanceAfter { get; init; }
    public required string Reference { get; init; }
    public int NrOfTransactions { get; set; }

    // ReSharper restore UnusedAutoPropertyAccessor.Global
 }