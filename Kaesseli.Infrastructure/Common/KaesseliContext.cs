using Kaesseli.Domain.Budget;
using Kaesseli.Domain.Common;
using Kaesseli.Domain.Journal;
using Microsoft.EntityFrameworkCore;

namespace Kaesseli.Infrastructure.Common;

public class KaesseliContext : DbContext
{
    public KaesseliContext(DbContextOptions<KaesseliContext> options)
        : base(options)
    {
    }

    public required DbSet<JournalEntry> JournalEntries { get; init; }
    public required DbSet<BudgetEntry> BudgetEntries { get; init; }
    public required DbSet<Account> Accounts { get; init; }


}