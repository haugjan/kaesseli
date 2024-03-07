using Kaesseli.Domain.Accounts;
using Kaesseli.Domain.Budget;
using Kaesseli.Domain.Integration;
using Kaesseli.Domain.Journal;
using Kaesseli.Domain.Prediction;
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
    public virtual DbSet<Transaction> PaymentEntries { get; init; } = null!;
    public virtual DbSet<BudgetEntry> BudgetEntries { get; init; } = null!;
    public virtual DbSet<Account> Accounts { get; init; } = null!;
    public virtual DbSet<TransactionSummary> TransactionSummaries { get; init; } = null!;
    public virtual DbSet<Transaction> Transactions { get; init; } = null!;
    public virtual DbSet<LearnedPrediction> LearnedPredictions { get; init; } = null!;
    public virtual DbSet<AccountingPeriod> AccountingPeriods { get; init; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<BudgetEntry>(
            entity =>
            {
                //entity.HasIndex(b => new { b.Account, b.AccountingPeriod })
                //      .IsUnique();

                entity.HasOne(be => be.Account)
                                .WithMany()
                                .IsRequired()
                                .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(be => be.AccountingPeriod)
                      .WithMany()
                      .IsRequired()
                      .OnDelete(DeleteBehavior.NoAction);
            });
        ;
        modelBuilder.Entity<JournalEntry>(
            entity =>
            {
                entity.HasOne(je => je.DebitAccount)
                      .WithMany() 
                      .HasForeignKey("DebitAccountId") 
                      .IsRequired()
                      .OnDelete(DeleteBehavior.NoAction); 

                entity.HasOne(je => je.CreditAccount)
                      .WithMany() 
                      .HasForeignKey("CreditAccountId") 
                      .IsRequired()
                      .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(je => je.Transaction)
                      .WithMany(tran => tran.JournalEntries);  
            });

        modelBuilder.Entity<TransactionSummary>(
            entity =>
            {
                entity.HasOne(je => je.Account)
                      .WithMany()
                      .HasForeignKey("AccountId")
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasMany(statement => statement.Transactions)
                      .WithOne(x=> x.TransactionSummary);

            });
    }
}