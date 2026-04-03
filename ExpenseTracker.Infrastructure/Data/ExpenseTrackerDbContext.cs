using ExpenseTracker.Infrastructure.Data.Entities;
using ExpenseTracker.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExpenseTracker.Infrastructure.Data;

public sealed class ExpenseTrackerDbContext(DbContextOptions<ExpenseTrackerDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole, string>(options)
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
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ApplicationUser>().ToTable("users");
        modelBuilder.Entity<IdentityRole>().ToTable("roles");
        modelBuilder.Entity<IdentityUserRole<string>>().ToTable("user_roles");
        modelBuilder.Entity<IdentityUserClaim<string>>().ToTable("user_claims");
        modelBuilder.Entity<IdentityUserLogin<string>>().ToTable("user_logins");
        modelBuilder.Entity<IdentityUserToken<string>>().ToTable("user_tokens");
        modelBuilder.Entity<IdentityRoleClaim<string>>().ToTable("role_claims");

        static void bookTable<T>(ModelBuilder mb, string name, Action<EntityTypeBuilder<T>>? configure = null)
            where T : class
        {
            var entity = mb.Entity<T>();
            entity.ToTable(name);
            configure?.Invoke(entity);
        }

        bookTable<UserBookMetadataEntity>(modelBuilder, "user_book_metadata", e =>
        {
            e.HasKey(x => x.UserId);
            e.Property(x => x.UserId).HasMaxLength(450);
            e.Property(x => x.BookRevision).IsRequired();
            e.Property(x => x.UpdatedAtUtc).IsRequired();
            e.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        bookTable<UserPreferencesEntity>(modelBuilder, "user_preferences", e =>
        {
            e.HasKey(x => x.UserId);
            e.Property(x => x.UserId).HasMaxLength(450);
            e.Property(x => x.Locale).HasMaxLength(32);
            e.Property(x => x.DefaultCurrencyCode).HasMaxLength(16);
            e.Property(x => x.LastPaymentInstrumentId).HasMaxLength(128);
            e.HasOne<PaymentInstrumentEntity>()
                .WithMany()
                .HasForeignKey(x => new { x.UserId, x.LastPaymentInstrumentId })
                .HasPrincipalKey(x => new { x.UserId, x.Id })
                .IsRequired(false)
                .OnDelete(DeleteBehavior.ClientSetNull);
            e.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        bookTable<PaymentInstrumentEntity>(modelBuilder, "payment_instruments", e =>
        {
            e.HasKey(x => new { x.UserId, x.Id });
            e.Property(x => x.UserId).HasMaxLength(450);
            e.Property(x => x.Id).HasMaxLength(128);
            e.Property(x => x.Label).HasMaxLength(512).IsRequired();
            e.Property(x => x.BankName).HasMaxLength(512);
            e.Property(x => x.FeeDescription).IsRequired();
            e.Property(x => x.DisplaySuffix).HasMaxLength(64);
            e.Property(x => x.AnnualFeeAmount).HasPrecision(19, 4);
            e.Property(x => x.MonthlyFeeAmount).HasPrecision(19, 4);
            e.Property(x => x.NominalAprPercent).HasPrecision(19, 6);
            e.Property(x => x.CreditLimit).HasPrecision(19, 4);
            e.HasIndex(x => x.UserId);
            e.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        bookTable<CategoryEntity>(modelBuilder, "categories", e =>
        {
            e.HasKey(x => new { x.UserId, x.Id });
            e.Property(x => x.UserId).HasMaxLength(450);
            e.Property(x => x.Id).HasMaxLength(128);
            e.Property(x => x.Name).HasMaxLength(512).IsRequired();
            e.HasIndex(x => x.UserId);
            e.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        bookTable<SubcategoryEntity>(modelBuilder, "subcategories", e =>
        {
            e.HasKey(x => new { x.UserId, x.Id });
            e.Property(x => x.UserId).HasMaxLength(450);
            e.Property(x => x.Id).HasMaxLength(128);
            e.Property(x => x.CategoryId).HasMaxLength(128).IsRequired();
            e.Property(x => x.Name).HasMaxLength(512).IsRequired();
            e.Property(x => x.Slug).HasMaxLength(256).IsRequired();
            e.HasOne<CategoryEntity>()
                .WithMany()
                .HasForeignKey(x => new { x.UserId, x.CategoryId })
                .HasPrincipalKey(x => new { x.UserId, x.Id })
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => new { x.UserId, x.CategoryId });
            e.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        bookTable<IncomeCategoryEntity>(modelBuilder, "income_categories", e =>
        {
            e.HasKey(x => new { x.UserId, x.Id });
            e.Property(x => x.UserId).HasMaxLength(450);
            e.Property(x => x.Id).HasMaxLength(128);
            e.Property(x => x.Name).HasMaxLength(512).IsRequired();
            e.HasIndex(x => x.UserId);
            e.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        bookTable<IncomeSubcategoryEntity>(modelBuilder, "income_subcategories", e =>
        {
            e.HasKey(x => new { x.UserId, x.Id });
            e.Property(x => x.UserId).HasMaxLength(450);
            e.Property(x => x.Id).HasMaxLength(128);
            e.Property(x => x.CategoryId).HasMaxLength(128).IsRequired();
            e.Property(x => x.Name).HasMaxLength(512).IsRequired();
            e.Property(x => x.Slug).HasMaxLength(256).IsRequired();
            e.HasOne<IncomeCategoryEntity>()
                .WithMany()
                .HasForeignKey(x => new { x.UserId, x.CategoryId })
                .HasPrincipalKey(x => new { x.UserId, x.Id })
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => new { x.UserId, x.CategoryId });
            e.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        bookTable<ExpenseRecurringSeriesEntity>(modelBuilder, "expense_recurring_series", e =>
        {
            e.HasKey(x => new { x.UserId, x.Id });
            e.Property(x => x.UserId).HasMaxLength(450);
            e.Property(x => x.Id).HasMaxLength(128);
            e.Property(x => x.AnchorOccurredOn).HasMaxLength(32).IsRequired();
            e.Property(x => x.CurrencyCode).HasMaxLength(16).IsRequired();
            e.Property(x => x.AmountOriginal).HasPrecision(19, 4);
            e.Property(x => x.ManualFxRateToUsd).HasPrecision(19, 8);
            e.Property(x => x.AmountUsd).HasPrecision(19, 4);
            e.Property(x => x.PaymentInstrumentId).HasMaxLength(128);
            e.HasOne<CategoryEntity>()
                .WithMany()
                .HasForeignKey(x => new { x.UserId, x.CategoryId })
                .HasPrincipalKey(x => new { x.UserId, x.Id })
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne<SubcategoryEntity>()
                .WithMany()
                .HasForeignKey(x => new { x.UserId, x.SubcategoryId })
                .HasPrincipalKey(x => new { x.UserId, x.Id })
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne<PaymentInstrumentEntity>()
                .WithMany()
                .HasForeignKey(x => new { x.UserId, x.PaymentInstrumentId })
                .HasPrincipalKey(x => new { x.UserId, x.Id })
                .OnDelete(DeleteBehavior.ClientSetNull);
            e.HasIndex(x => x.UserId);
            e.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        bookTable<IncomeRecurringSeriesEntity>(modelBuilder, "income_recurring_series", e =>
        {
            e.HasKey(x => new { x.UserId, x.Id });
            e.Property(x => x.UserId).HasMaxLength(450);
            e.Property(x => x.Id).HasMaxLength(128);
            e.Property(x => x.AnchorReceivedOn).HasMaxLength(32).IsRequired();
            e.Property(x => x.CurrencyCode).HasMaxLength(16).IsRequired();
            e.Property(x => x.AmountOriginal).HasPrecision(19, 4);
            e.Property(x => x.ManualFxRateToUsd).HasPrecision(19, 8);
            e.Property(x => x.AmountUsd).HasPrecision(19, 4);
            e.HasOne<IncomeCategoryEntity>()
                .WithMany()
                .HasForeignKey(x => new { x.UserId, x.IncomeCategoryId })
                .HasPrincipalKey(x => new { x.UserId, x.Id })
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne<IncomeSubcategoryEntity>()
                .WithMany()
                .HasForeignKey(x => new { x.UserId, x.IncomeSubcategoryId })
                .HasPrincipalKey(x => new { x.UserId, x.Id })
                .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => x.UserId);
            e.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        bookTable<InstallmentPlanEntity>(modelBuilder, "installment_plans", e =>
        {
            e.HasKey(x => new { x.UserId, x.Id });
            e.Property(x => x.UserId).HasMaxLength(450);
            e.Property(x => x.Id).HasMaxLength(128);
            e.Property(x => x.AnchorOccurredOn).HasMaxLength(32).IsRequired();
            e.Property(x => x.CurrencyCode).HasMaxLength(16).IsRequired();
            e.Property(x => x.PerPaymentAmountOriginal).HasPrecision(19, 4);
            e.Property(x => x.ManualFxRateToUsd).HasPrecision(19, 8);
            e.Property(x => x.PerPaymentAmountUsd).HasPrecision(19, 4);
            e.Property(x => x.PaymentInstrumentId).HasMaxLength(128);
            e.HasOne<CategoryEntity>()
                .WithMany()
                .HasForeignKey(x => new { x.UserId, x.CategoryId })
                .HasPrincipalKey(x => new { x.UserId, x.Id })
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne<SubcategoryEntity>()
                .WithMany()
                .HasForeignKey(x => new { x.UserId, x.SubcategoryId })
                .HasPrincipalKey(x => new { x.UserId, x.Id })
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne<PaymentInstrumentEntity>()
                .WithMany()
                .HasForeignKey(x => new { x.UserId, x.PaymentInstrumentId })
                .HasPrincipalKey(x => new { x.UserId, x.Id })
                .OnDelete(DeleteBehavior.ClientSetNull);
            e.HasIndex(x => x.UserId);
            e.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        bookTable<ExpenseEntity>(modelBuilder, "expenses", e =>
        {
            e.HasKey(x => new { x.UserId, x.Id });
            e.Property(x => x.UserId).HasMaxLength(450);
            e.Property(x => x.Id).HasMaxLength(128);
            e.Property(x => x.OccurredOn).HasMaxLength(32).IsRequired();
            e.Property(x => x.CategoryId).HasMaxLength(128).IsRequired();
            e.Property(x => x.SubcategoryId).HasMaxLength(128).IsRequired();
            e.Property(x => x.CurrencyCode).HasMaxLength(16).IsRequired();
            e.Property(x => x.AmountOriginal).HasPrecision(19, 4);
            e.Property(x => x.ManualFxRateToUsd).HasPrecision(19, 8);
            e.Property(x => x.AmountUsd).HasPrecision(19, 4);
            e.Property(x => x.PaymentInstrumentId).HasMaxLength(128);
            e.Property(x => x.RecurringSeriesId).HasMaxLength(128);
            e.Property(x => x.PaymentExpectationStatus).HasMaxLength(64);
            e.Property(x => x.PaymentExpectationConfirmedOn).HasMaxLength(32);
            e.Property(x => x.InstallmentPlanId).HasMaxLength(128);
            e.HasOne<CategoryEntity>()
                .WithMany()
                .HasForeignKey(x => new { x.UserId, x.CategoryId })
                .HasPrincipalKey(x => new { x.UserId, x.Id })
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne<SubcategoryEntity>()
                .WithMany()
                .HasForeignKey(x => new { x.UserId, x.SubcategoryId })
                .HasPrincipalKey(x => new { x.UserId, x.Id })
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne<PaymentInstrumentEntity>()
                .WithMany()
                .HasForeignKey(x => new { x.UserId, x.PaymentInstrumentId })
                .HasPrincipalKey(x => new { x.UserId, x.Id })
                .OnDelete(DeleteBehavior.ClientSetNull);
            e.HasOne<ExpenseRecurringSeriesEntity>()
                .WithMany()
                .HasForeignKey(x => new { x.UserId, x.RecurringSeriesId })
                .HasPrincipalKey(x => new { x.UserId, x.Id })
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne<InstallmentPlanEntity>()
                .WithMany()
                .HasForeignKey(x => new { x.UserId, x.InstallmentPlanId })
                .HasPrincipalKey(x => new { x.UserId, x.Id })
                .OnDelete(DeleteBehavior.ClientSetNull);
            e.HasIndex(x => new { x.UserId, x.OccurredOn });
            e.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        bookTable<IncomeEntryEntity>(modelBuilder, "income_entries", e =>
        {
            e.HasKey(x => new { x.UserId, x.Id });
            e.Property(x => x.UserId).HasMaxLength(450);
            e.Property(x => x.Id).HasMaxLength(128);
            e.Property(x => x.ReceivedOn).HasMaxLength(32).IsRequired();
            e.Property(x => x.IncomeCategoryId).HasMaxLength(128).IsRequired();
            e.Property(x => x.IncomeSubcategoryId).HasMaxLength(128).IsRequired();
            e.Property(x => x.CurrencyCode).HasMaxLength(16).IsRequired();
            e.Property(x => x.AmountOriginal).HasPrecision(19, 4);
            e.Property(x => x.ManualFxRateToUsd).HasPrecision(19, 8);
            e.Property(x => x.AmountUsd).HasPrecision(19, 4);
            e.Property(x => x.RecurringSeriesId).HasMaxLength(128);
            e.Property(x => x.ExpectationStatus).HasMaxLength(64);
            e.Property(x => x.ExpectationConfirmedOn).HasMaxLength(32);
            e.HasOne<IncomeCategoryEntity>()
                .WithMany()
                .HasForeignKey(x => new { x.UserId, x.IncomeCategoryId })
                .HasPrincipalKey(x => new { x.UserId, x.Id })
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne<IncomeSubcategoryEntity>()
                .WithMany()
                .HasForeignKey(x => new { x.UserId, x.IncomeSubcategoryId })
                .HasPrincipalKey(x => new { x.UserId, x.Id })
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne<IncomeRecurringSeriesEntity>()
                .WithMany()
                .HasForeignKey(x => new { x.UserId, x.RecurringSeriesId })
                .HasPrincipalKey(x => new { x.UserId, x.Id })
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => new { x.UserId, x.ReceivedOn });
            e.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
