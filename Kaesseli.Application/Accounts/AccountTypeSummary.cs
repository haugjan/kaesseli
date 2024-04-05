namespace Kaesseli.Application.Accounts;

public class AccountTypeSummary
{
    private readonly decimal? _budget;
    private decimal? _currentBudget;
    private readonly decimal? _budgetBalance;
    private readonly decimal? _budgetPerMonth;
    private readonly decimal? _budgetPerYear;

    // ReSharper disable UnusedAutoPropertyAccessor.Global
    // ReSharper disable UnusedMember.Global
    public required decimal AccountBalance { get; init; }

    public required decimal? Budget
    {
        get => _budget;
        init => _budget = value != 0 ? value : null;
    }

    public required decimal? BudgetPerMonth
    {
        get => _budgetPerYear;
        init => _budgetPerYear = value != 0 ? value : null;
    }

    public required decimal? BudgetPerYear
    {
        get => _budgetPerMonth;
        init => _budgetPerMonth = value != 0 ? value : null;
    }

    public required decimal? CurrentBudget
    {
        get => _currentBudget;
        set => _currentBudget = value != 0 ? value : null;
    }

    public required decimal? BudgetBalance
    {
        get => _budgetBalance;
        init => _budgetBalance = value != 0 ? value : null;
    }
    // ReSharper restore UnusedAutoPropertyAccessor.Global
    // ReSharper restore UnusedMember.Global

}