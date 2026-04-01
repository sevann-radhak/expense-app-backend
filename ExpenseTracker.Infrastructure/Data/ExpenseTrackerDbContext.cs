using ExpenseTracker.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpenseTracker.Infrastructure.Data;

public sealed class ExpenseTrackerDbContext(DbContextOptions<ExpenseTrackerDbContext> options)
    : DbContext(options)
{
    public DbSet<UserBookMetadataEntity> UserBookMetadata => Set<UserBookMetadataEntity>();
    public DbSet<UserPreferencesEntity> UserPreferences => Set<UserPreferencesEntity>();
    public DbSet<PaymentInstrumentEntity> PaymentInstruments => Set<PaymentInstrumentEntity>();
    public DbSet<CategoryEntity> Categories => Set<CategoryEntity>();
    public DbSet<SubcategoryEntity> Subcategories => Set<SubcategoryEntity>();
    public DbSet<IncomeCategoryEntity> IncomeCategories => Set<IncomeCategoryEntity>();
    public DbSet<IncomeSubcategoryEntity> IncomeSubcategories => Set<IncomeSubcategoryEntity>();
    public DbSet<ExpenseRecurringSeriesEntity> ExpenseRecurringSeries => Set<ExpenseRecurringSeriesEntity>();
    public DbSet<IncomeRecurringSeriesEntity> IncomeRecurringSeries => Set<IncomeRecurringSeriesEntity>();
    public DbSet<InstallmentPlanEntity> InstallmentPlans => Set<InstallmentPlanEntity>();
    public DbSet<ExpenseEntity> Expenses => Set<ExpenseEntity>();
    public DbSet<IncomeEntryEntity> IncomeEntries => Set<IncomeEntryEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        static void bookTable<T>(ModelBuilder mb, string name, Action<EntityTypeBuilder<T>>? extra = null)
            where T : class
        {
            global::Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<T> e = mb.Entity<T>();
            _ = e.ToTable(name);
            extra?.Invoke(e);
        }

        bookTable<UserBookMetadataEntity>(modelBuilder, "user_book_metadata", e =>
        {
            _ = e.HasKey(x => x.UserId);
            _ = e.Property(x => x.UserId).HasMaxLength(450);
            _ = e.Property(x => x.BookRevision).IsRequired();
            _ = e.Property(x => x.UpdatedAtUtc).IsRequired();
        });

        bookTable<UserPreferencesEntity>(modelBuilder, "user_preferences", e =>
        {
            _ = e.HasKey(x => x.UserId);
            _ = e.Property(x => x.UserId).HasMaxLength(450);
            _ = e.Property(x => x.Locale).HasMaxLength(32);
            _ = e.Property(x => x.DefaultCurrencyCode).HasMaxLength(16);
            _ = e.Property(x => x.LastPaymentInstrumentId).HasMaxLength(128);
            // Composite FK includes non-nullable UserId — SQL Server rejects ON DELETE SET NULL.
            // ClientSetNull maps to NO ACTION in the database; EF clears LastPaymentInstrumentId when the PI is deleted in the change tracker.
            _ = e.HasOne<PaymentInstrumentEntity>()
                .WithMany()
                .HasForeignKey(x => new { x.UserId, x.LastPaymentInstrumentId })
                .HasPrincipalKey(x => new { x.UserId, x.Id })
                .IsRequired(false)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        bookTable<PaymentInstrumentEntity>(modelBuilder, "payment_instruments", e =>
        {
            _ = e.HasKey(x => new { x.UserId, x.Id });
            _ = e.Property(x => x.UserId).HasMaxLength(450);
            _ = e.Property(x => x.Id).HasMaxLength(128);
            _ = e.Property(x => x.Label).HasMaxLength(512).IsRequired();
            _ = e.Property(x => x.BankName).HasMaxLength(512);
            _ = e.Property(x => x.FeeDescription).IsRequired();
            _ = e.Property(x => x.DisplaySuffix).HasMaxLength(64);
            _ = e.Property(x => x.AnnualFeeAmount).HasPrecision(19, 4);
            _ = e.Property(x => x.MonthlyFeeAmount).HasPrecision(19, 4);
            _ = e.Property(x => x.NominalAprPercent).HasPrecision(19, 6);
            _ = e.Property(x => x.CreditLimit).HasPrecision(19, 4);
            _ = e.HasIndex(x => x.UserId);
        });

        bookTable<CategoryEntity>(modelBuilder, "categories", e =>
        {
            _ = e.HasKey(x => new { x.UserId, x.Id });
            _ = e.Property(x => x.UserId).HasMaxLength(450);
            _ = e.Property(x => x.Id).HasMaxLength(128);
            _ = e.Property(x => x.Name).HasMaxLength(512).IsRequired();
            _ = e.HasIndex(x => x.UserId);
        });

        bookTable<SubcategoryEntity>(modelBuilder, "subcategories", e =>
        {
            _ = e.HasKey(x => new { x.UserId, x.Id });
            _ = e.Property(x => x.UserId).HasMaxLength(450);
            _ = e.Property(x => x.Id).HasMaxLength(128);
            _ = e.Property(x => x.CategoryId).HasMaxLength(128).IsRequired();
            _ = e.Property(x => x.Name).HasMaxLength(512).IsRequired();
            _ = e.Property(x => x.Slug).HasMaxLength(256).IsRequired();
            _ = e.HasOne<CategoryEntity>()
                .WithMany()
                .HasForeignKey(x => new { x.UserId, x.CategoryId })
                .HasPrincipalKey(x => new { x.UserId, x.Id })
                .OnDelete(DeleteBehavior.Cascade);
            _ = e.HasIndex(x => new { x.UserId, x.CategoryId });
        });

        bookTable<IncomeCategoryEntity>(modelBuilder, "income_categories", e =>
        {
            _ = e.HasKey(x => new { x.UserId, x.Id });
            _ = e.Property(x => x.UserId).HasMaxLength(450);
            _ = e.Property(x => x.Id).HasMaxLength(128);
            _ = e.Property(x => x.Name).HasMaxLength(512).IsRequired();
            _ = e.HasIndex(x => x.UserId);
        });

        bookTable<IncomeSubcategoryEntity>(modelBuilder, "income_subcategories", e =>
        {
            _ = e.HasKey(x => new { x.UserId, x.Id });
            _ = e.Property(x => x.UserId).HasMaxLength(450);
            _ = e.Property(x => x.Id).HasMaxLength(128);
            _ = e.Property(x => x.CategoryId).HasMaxLength(128).IsRequired();
            _ = e.Property(x => x.Name).HasMaxLength(512).IsRequired();
            _ = e.Property(x => x.Slug).HasMaxLength(256).IsRequired();
            _ = e.HasOne<IncomeCategoryEntity>()
                .WithMany()
                .HasForeignKey(x => new { x.UserId, x.CategoryId })
                .HasPrincipalKey(x => new { x.UserId, x.Id })
                .OnDelete(DeleteBehavior.Cascade);
            _ = e.HasIndex(x => new { x.UserId, x.CategoryId });
        });

        bookTable<ExpenseRecurringSeriesEntity>(modelBuilder, "expense_recurring_series", e =>
        {
            _ = e.HasKey(x => new { x.UserId, x.Id });
            _ = e.Property(x => x.UserId).HasMaxLength(450);
            _ = e.Property(x => x.Id).HasMaxLength(128);
            _ = e.Property(x => x.AnchorOccurredOn).HasMaxLength(32).IsRequired();
            _ = e.Property(x => x.CurrencyCode).HasMaxLength(16).IsRequired();
            _ = e.Property(x => x.AmountOriginal).HasPrecision(19, 4);
            _ = e.Property(x => x.ManualFxRateToUsd).HasPrecision(19, 8);
            _ = e.Property(x => x.AmountUsd).HasPrecision(19, 4);
            _ = e.Property(x => x.PaymentInstrumentId).HasMaxLength(128);
            _ = e.HasOne<CategoryEntity>()
                .WithMany()
                .HasForeignKey(x => new { x.UserId, x.CategoryId })
                .HasPrincipalKey(x => new { x.UserId, x.Id })
                .OnDelete(DeleteBehavior.Restrict);
            _ = e.HasOne<SubcategoryEntity>()
                .WithMany()
                .HasForeignKey(x => new { x.UserId, x.SubcategoryId })
                .HasPrincipalKey(x => new { x.UserId, x.Id })
                .OnDelete(DeleteBehavior.Restrict);
            _ = e.HasOne<PaymentInstrumentEntity>()
                .WithMany()
                .HasForeignKey(x => new { x.UserId, x.PaymentInstrumentId })
                .HasPrincipalKey(x => new { x.UserId, x.Id })
                .OnDelete(DeleteBehavior.ClientSetNull);
            _ = e.HasIndex(x => x.UserId);
        });

        bookTable<IncomeRecurringSeriesEntity>(modelBuilder, "income_recurring_series", e =>
        {
            _ = e.HasKey(x => new { x.UserId, x.Id });
            _ = e.Property(x => x.UserId).HasMaxLength(450);
            _ = e.Property(x => x.Id).HasMaxLength(128);
            _ = e.Property(x => x.AnchorReceivedOn).HasMaxLength(32).IsRequired();
            _ = e.Property(x => x.CurrencyCode).HasMaxLength(16).IsRequired();
            _ = e.Property(x => x.AmountOriginal).HasPrecision(19, 4);
            _ = e.Property(x => x.ManualFxRateToUsd).HasPrecision(19, 8);
            _ = e.Property(x => x.AmountUsd).HasPrecision(19, 4);
            _ = e.HasOne<IncomeCategoryEntity>()
                .WithMany()
                .HasForeignKey(x => new { x.UserId, x.IncomeCategoryId })
                .HasPrincipalKey(x => new { x.UserId, x.Id })
                .OnDelete(DeleteBehavior.Restrict);
            _ = e.HasOne<IncomeSubcategoryEntity>()
                .WithMany()
                .HasForeignKey(x => new { x.UserId, x.IncomeSubcategoryId })
                .HasPrincipalKey(x => new { x.UserId, x.Id })
                .OnDelete(DeleteBehavior.Restrict);
            _ = e.HasIndex(x => x.UserId);
        });

        bookTable<InstallmentPlanEntity>(modelBuilder, "installment_plans", e =>
        {
            _ = e.HasKey(x => new { x.UserId, x.Id });
            _ = e.Property(x => x.UserId).HasMaxLength(450);
            _ = e.Property(x => x.Id).HasMaxLength(128);
            _ = e.Property(x => x.AnchorOccurredOn).HasMaxLength(32).IsRequired();
            _ = e.Property(x => x.CurrencyCode).HasMaxLength(16).IsRequired();
            _ = e.Property(x => x.PerPaymentAmountOriginal).HasPrecision(19, 4);
            _ = e.Property(x => x.ManualFxRateToUsd).HasPrecision(19, 8);
            _ = e.Property(x => x.PerPaymentAmountUsd).HasPrecision(19, 4);
            _ = e.Property(x => x.PaymentInstrumentId).HasMaxLength(128);
            _ = e.HasOne<CategoryEntity>()
                .WithMany()
                .HasForeignKey(x => new { x.UserId, x.CategoryId })
                .HasPrincipalKey(x => new { x.UserId, x.Id })
                .OnDelete(DeleteBehavior.Restrict);
            _ = e.HasOne<SubcategoryEntity>()
                .WithMany()
                .HasForeignKey(x => new { x.UserId, x.SubcategoryId })
                .HasPrincipalKey(x => new { x.UserId, x.Id })
                .OnDelete(DeleteBehavior.Restrict);
            _ = e.HasOne<PaymentInstrumentEntity>()
                .WithMany()
                .HasForeignKey(x => new { x.UserId, x.PaymentInstrumentId })
                .HasPrincipalKey(x => new { x.UserId, x.Id })
                .OnDelete(DeleteBehavior.ClientSetNull);
            _ = e.HasIndex(x => x.UserId);
        });

        bookTable<ExpenseEntity>(modelBuilder, "expenses", e =>
        {
            _ = e.HasKey(x => new { x.UserId, x.Id });
            _ = e.Property(x => x.UserId).HasMaxLength(450);
            _ = e.Property(x => x.Id).HasMaxLength(128);
            _ = e.Property(x => x.OccurredOn).HasMaxLength(32).IsRequired();
            _ = e.Property(x => x.CategoryId).HasMaxLength(128).IsRequired();
            _ = e.Property(x => x.SubcategoryId).HasMaxLength(128).IsRequired();
            _ = e.Property(x => x.CurrencyCode).HasMaxLength(16).IsRequired();
            _ = e.Property(x => x.AmountOriginal).HasPrecision(19, 4);
            _ = e.Property(x => x.ManualFxRateToUsd).HasPrecision(19, 8);
            _ = e.Property(x => x.AmountUsd).HasPrecision(19, 4);
            _ = e.Property(x => x.PaymentInstrumentId).HasMaxLength(128);
            _ = e.Property(x => x.RecurringSeriesId).HasMaxLength(128);
            _ = e.Property(x => x.PaymentExpectationStatus).HasMaxLength(64);
            _ = e.Property(x => x.PaymentExpectationConfirmedOn).HasMaxLength(32);
            _ = e.Property(x => x.InstallmentPlanId).HasMaxLength(128);
            _ = e.HasOne<CategoryEntity>()
                .WithMany()
                .HasForeignKey(x => new { x.UserId, x.CategoryId })
                .HasPrincipalKey(x => new { x.UserId, x.Id })
                .OnDelete(DeleteBehavior.Restrict);
            _ = e.HasOne<SubcategoryEntity>()
                .WithMany()
                .HasForeignKey(x => new { x.UserId, x.SubcategoryId })
                .HasPrincipalKey(x => new { x.UserId, x.Id })
                .OnDelete(DeleteBehavior.Restrict);
            _ = e.HasOne<PaymentInstrumentEntity>()
                .WithMany()
                .HasForeignKey(x => new { x.UserId, x.PaymentInstrumentId })
                .HasPrincipalKey(x => new { x.UserId, x.Id })
                .OnDelete(DeleteBehavior.ClientSetNull);
            _ = e.HasOne<ExpenseRecurringSeriesEntity>()
                .WithMany()
                .HasForeignKey(x => new { x.UserId, x.RecurringSeriesId })
                .HasPrincipalKey(x => new { x.UserId, x.Id })
                .OnDelete(DeleteBehavior.Cascade);
            _ = e.HasOne<InstallmentPlanEntity>()
                .WithMany()
                .HasForeignKey(x => new { x.UserId, x.InstallmentPlanId })
                .HasPrincipalKey(x => new { x.UserId, x.Id })
                .OnDelete(DeleteBehavior.ClientSetNull);
            _ = e.HasIndex(x => new { x.UserId, x.OccurredOn });
        });

        bookTable<IncomeEntryEntity>(modelBuilder, "income_entries", e =>
        {
            _ = e.HasKey(x => new { x.UserId, x.Id });
            _ = e.Property(x => x.UserId).HasMaxLength(450);
            _ = e.Property(x => x.Id).HasMaxLength(128);
            _ = e.Property(x => x.ReceivedOn).HasMaxLength(32).IsRequired();
            _ = e.Property(x => x.IncomeCategoryId).HasMaxLength(128).IsRequired();
            _ = e.Property(x => x.IncomeSubcategoryId).HasMaxLength(128).IsRequired();
            _ = e.Property(x => x.CurrencyCode).HasMaxLength(16).IsRequired();
            _ = e.Property(x => x.AmountOriginal).HasPrecision(19, 4);
            _ = e.Property(x => x.ManualFxRateToUsd).HasPrecision(19, 8);
            _ = e.Property(x => x.AmountUsd).HasPrecision(19, 4);
            _ = e.Property(x => x.RecurringSeriesId).HasMaxLength(128);
            _ = e.Property(x => x.ExpectationStatus).HasMaxLength(64);
            _ = e.Property(x => x.ExpectationConfirmedOn).HasMaxLength(32);
            _ = e.HasOne<IncomeCategoryEntity>()
                .WithMany()
                .HasForeignKey(x => new { x.UserId, x.IncomeCategoryId })
                .HasPrincipalKey(x => new { x.UserId, x.Id })
                .OnDelete(DeleteBehavior.Restrict);
            _ = e.HasOne<IncomeSubcategoryEntity>()
                .WithMany()
                .HasForeignKey(x => new { x.UserId, x.IncomeSubcategoryId })
                .HasPrincipalKey(x => new { x.UserId, x.Id })
                .OnDelete(DeleteBehavior.Restrict);
            _ = e.HasOne<IncomeRecurringSeriesEntity>()
                .WithMany()
                .HasForeignKey(x => new { x.UserId, x.RecurringSeriesId })
                .HasPrincipalKey(x => new { x.UserId, x.Id })
                .OnDelete(DeleteBehavior.Cascade);
            _ = e.HasIndex(x => new { x.UserId, x.ReceivedOn });
        });
    }
}
