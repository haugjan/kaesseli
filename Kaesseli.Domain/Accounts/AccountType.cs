namespace Kaesseli.Domain.Accounts;

public enum AccountType
{
    Asset = 1,
    Liability = 2,
    Revenue = 3,
    Expense = 4
}

public enum ParentAccountType
{
    BalanceSheet = 1,
    IncomeStatement
}