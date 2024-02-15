using Kaesseli.Domain.Accounts;
using Kaesseli.Domain.Budget;
using Kaesseli.Domain.Journal;
using Microsoft.EntityFrameworkCore;

namespace Kaesseli.Infrastructure.Common;

public class KaesseliContext : DbContext
{
    [Obsolete(message: "Should only be used by Unit-Tests")]
    public KaesseliContext()
    {
    }

    public KaesseliContext(DbContextOptions<KaesseliContext> options)
        : base(options)
    {
    }

    public virtual DbSet<JournalEntry> JournalEntries { get; init; } = null!;
    public virtual DbSet<BudgetEntry> BudgetEntries { get; init; } = null!;
    public virtual DbSet<Account> Accounts { get; init; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Konfigurieren der JournalEntry-Entität
        modelBuilder.Entity<JournalEntry>(
            entity =>
            {
                entity.HasOne(je => je.DebitAccount)
                      .WithMany() 
                      .HasForeignKey("DebitAccountId") 
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Restrict); 

                entity.HasOne(je => je.CreditAccount)
                      .WithMany() 
                      .HasForeignKey("CreditAccountId") 
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Restrict);

            });
    }
}