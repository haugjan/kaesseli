using Kaesseli.Features.Accounts;
using Kaesseli.Features.Budget;
using Kaesseli.Features.Integration;
using Kaesseli.Features.Journal;
using Microsoft.EntityFrameworkCore;
using Kaesseli.Features.Automation;

namespace Kaesseli.Infrastructure;

public class KaesseliContext : DbContext
{
    private const string InsertDateColumnName = "InsertDate";
    private const string EditDateColumnName = "EditDate";
    private const string InsertUserColumnName = "InsertUser";
    private const string EditUserColumnName = "EditUser";
    private readonly TimeProvider _timeProvider;
    private readonly IEnvironmentService _environmentService;

    [Obsolete(message: "Should only be used by Unit-Tests")]
    public KaesseliContext()
    {
        _environmentService = null!;
        _timeProvider = null!;
    }

    public KaesseliContext(DbContextOptions<KaesseliContext> options, TimeProvider timeProvider, IEnvironmentService environmentService)
        : base(options)
    {
        _timeProvider = timeProvider;
        _environmentService = environmentService;
    }

    public virtual DbSet<JournalEntry> JournalEntries { get; init; } = null!;
    public virtual DbSet<Transaction> PaymentEntries { get; init; } = null!;
    public virtual DbSet<BudgetEntry> BudgetEntries { get; init; } = null!;
    public virtual DbSet<Account> Accounts { get; init; } = null!;
    public virtual DbSet<TransactionSummary> TransactionSummaries { get; init; } = null!;
    public virtual DbSet<Transaction> Transactions { get; init; } = null!;
    public virtual DbSet<AccountingPeriod> AccountingPeriods { get; init; } = null!;
    public virtual DbSet<TransactionStatistic> TransactionStatistics { get; init; } = null!;
    public virtual DbSet<AutomationEntry> Automations { get; init; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Account>(entity =>
        {
            entity.ToContainer("Accounts");
            entity.HasPartitionKey(a => a.Id);
            entity.Property(a => a.Type).HasConversion<string>();
            entity.OwnsOne(a => a.Icon);
        });

        modelBuilder.Entity<AccountingPeriod>(entity =>
        {
            entity.ToContainer("AccountingPeriods");
            entity.HasPartitionKey(a => a.Id);
        });

        modelBuilder.Entity<AutomationEntry>(entity =>
        {
            entity.ToContainer("Automations");
            entity.HasPartitionKey(a => a.Id);
            entity.HasMany(ae => ae.Parts).WithOne().IsRequired();
        });

        modelBuilder.Entity<AutomationEntryPart>(entity =>
        {
            entity.ToContainer("AutomationEntryParts");
            entity.HasPartitionKey(a => a.Id);
        });

        modelBuilder.Entity<BudgetEntry>(entity =>
        {
            entity.ToContainer("BudgetEntries");
            entity.HasPartitionKey(b => b.Id);
            entity.HasOne(be => be.Account).WithMany().IsRequired();
            entity.HasOne(be => be.AccountingPeriod).WithMany().IsRequired();
        });

        modelBuilder.Entity<JournalEntry>(entity =>
        {
            entity.ToContainer("JournalEntries");
            entity.HasPartitionKey(j => j.Id);
            entity.HasOne(je => je.DebitAccount).WithMany().IsRequired();
            entity.HasOne(je => je.CreditAccount).WithMany().IsRequired();
            entity.HasOne(je => je.Transaction).WithMany(t => t.JournalEntries);
        });

        modelBuilder.Entity<TransactionSummary>(entity =>
        {
            entity.ToContainer("TransactionSummaries");
            entity.HasPartitionKey(ts => ts.Id);
            entity.HasOne(ts => ts.Account).WithMany().IsRequired();
            entity.HasMany(ts => ts.Transactions).WithOne(t => t.TransactionSummary);
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.ToContainer("Transactions");
            entity.HasPartitionKey(t => t.Id);
        });

        modelBuilder.Entity<TransactionStatistic>(entity =>
        {
            entity.ToContainer("TransactionStatistics");
            entity.HasPartitionKey(ts => ts.Id);
        });

        foreach (var entityType in modelBuilder.Model.GetEntityTypes().Where(e => !e.IsOwned()))
        {
            modelBuilder.Entity(entityType.ClrType).Property<DateTimeOffset>(InsertDateColumnName);
            modelBuilder.Entity(entityType.ClrType).Property<string>(InsertUserColumnName);
            modelBuilder.Entity(entityType.ClrType).Property<DateTimeOffset>(EditDateColumnName);
            modelBuilder.Entity(entityType.ClrType).Property<string>(EditUserColumnName);
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
                entry.Property(InsertDateColumnName).CurrentValue = _timeProvider.GetUtcNow();
                entry.Property(InsertUserColumnName).CurrentValue = _environmentService.CurrentUser;
            }

            if (entry.Metadata.FindProperty(EditDateColumnName) == null) continue;

            entry.Property(EditDateColumnName).CurrentValue = _timeProvider.GetUtcNow();
            entry.Property(EditUserColumnName).CurrentValue = _environmentService.CurrentUser;
        }
    }
}
