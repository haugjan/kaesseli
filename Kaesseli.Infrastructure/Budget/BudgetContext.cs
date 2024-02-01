using Kaesseli.Domain.Budget;
using Kaesseli.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kaesseli.Infrastructure.Budget;

public class BudgetContext : DbContext
{
    public DbSet<BudgetEntry> BudgetEntries { get; set; }
    public DbSet<Account> Accounts { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => 
        optionsBuilder.UseSqlite("Data Source=budget.db");
}