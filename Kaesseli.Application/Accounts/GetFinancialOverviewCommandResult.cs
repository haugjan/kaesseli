namespace Kaesseli.Application.Accounts;

public class GetFinancialOverviewCommandResult
{
    public required AccountTypeSummary Expense { get; init; }
    public required AccountTypeSummary Revenue { get; init; }
    public required AccountTypeSummary Liability { get; set; }
    public required AccountTypeSummary Asset { get; init; }
}