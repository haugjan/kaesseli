using Kaesseli.Domain.Accounts;
using Kaesseli.Domain.Budget;
using Kaesseli.Domain.Integration;
using Kaesseli.Domain.Journal;
using Microsoft.EntityFrameworkCore;
using Kaesseli.Application.Utility;

namespace Kaesseli.Infrastructure.Common;

public class KaesseliContext : DbContext
{
    private const string InsertDateColumnName = "InsertDate";
    private const string EditDateColumnName = "EditDate";
    private const string InsertUserColumnName = "InsertUser";
    private const string EditUserColumnName = "EditUser";
    private readonly IDateTimeService _dateTime;
    private readonly IEnvironmentService _environmentService;

    [Obsolete(message: "Should only be used by Unit-Tests")]
    public KaesseliContext()
    {
        _environmentService = null!;
        _dateTime = null!;
    }

    public KaesseliContext(DbContextOptions<KaesseliContext> options, IDateTimeService dateTime, IEnvironmentService environmentService)
        : base(options)
    {
        _dateTime = dateTime;
        _environmentService = environmentService;
    }

    public virtual DbSet<JournalEntry> JournalEntries { get; init; } = null!;
    public virtual DbSet<Transaction> PaymentEntries { get; init; } = null!;
    public virtual DbSet<BudgetEntry> BudgetEntries { get; init; } = null!;
    public virtual DbSet<Account> Accounts { get; init; } = null!;
    public virtual DbSet<TransactionSummary> TransactionSummaries { get; init; } = null!;
    public virtual DbSet<Transaction> Transactions { get; init; } = null!;
    public virtual DbSet<AccountingPeriod> AccountingPeriods { get; init; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<BudgetEntry>(
            entity =>
            {
                entity.HasOne(be => be.Account)
                      .WithMany()
                      .IsRequired()
                      .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(be => be.AccountingPeriod)
                      .WithMany()
                      .IsRequired()
                      .OnDelete(DeleteBehavior.NoAction);
            });
        
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
                      .WithOne(x => x.TransactionSummary);
            });

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // Hinzufügen von Shadow Properties
            modelBuilder.Entity(entityType.ClrType).Property<DateTimeOffset>(propertyName: InsertDateColumnName).HasDefaultValueSql(sql: "GETDATE()");
            modelBuilder.Entity(entityType.ClrType).Property<string>(propertyName: InsertUserColumnName);
            modelBuilder.Entity(entityType.ClrType).Property<DateTimeOffset>(propertyName: EditDateColumnName).HasDefaultValueSql(sql: "GETDATE()");
            modelBuilder.Entity(entityType.ClrType).Property<string>(propertyName: EditUserColumnName);
        }
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetShadowProperties();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void SetShadowProperties()
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State != EntityState.Added && entry.State != EntityState.Modified) continue;

            if (entry.Metadata.FindProperty(InsertDateColumnName) != null && entry.State == EntityState.Added)
            {
                entry.Property(InsertDateColumnName).CurrentValue = _dateTime.UtcNow;
                entry.Property(InsertUserColumnName).CurrentValue = _environmentService.CurrentUser;
            }

            if (entry.Metadata.FindProperty(EditDateColumnName) == null) continue;

            entry.Property(EditDateColumnName).CurrentValue = _dateTime.UtcNow;
            entry.Property(EditUserColumnName).CurrentValue = _environmentService.CurrentUser;
        }
    }
}