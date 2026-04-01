using ExpenseTracker.Infrastructure.Data;
using ExpenseTracker.Infrastructure.Data.Entities;
using ExpenseTracker.Infrastructure.Taxonomy;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Infrastructure.Services;

public sealed class DevBookDataService(ExpenseTrackerDbContext db)
{
    public static void ValidateUserId(string userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        if (userId.Length > 450)
        {
            throw new ArgumentException("userId exceeds 450 characters.", nameof(userId));
        }
    }

    public async Task ResetUserBookAsync(string userId, CancellationToken cancellationToken = default)
    {
        ValidateUserId(userId);
        _ = await db.Expenses.Where(e => e.UserId == userId).ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
        _ = await db.IncomeEntries.Where(e => e.UserId == userId).ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
        _ = await db.ExpenseRecurringSeries.Where(e => e.UserId == userId).ExecuteDeleteAsync(cancellationToken)
            .ConfigureAwait(false);
        _ = await db.IncomeRecurringSeries.Where(e => e.UserId == userId).ExecuteDeleteAsync(cancellationToken)
            .ConfigureAwait(false);
        _ = await db.InstallmentPlans.Where(e => e.UserId == userId).ExecuteDeleteAsync(cancellationToken)
            .ConfigureAwait(false);
        _ = await db.Subcategories.Where(e => e.UserId == userId).ExecuteDeleteAsync(cancellationToken)
            .ConfigureAwait(false);
        _ = await db.Categories.Where(e => e.UserId == userId).ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
        _ = await db.IncomeSubcategories.Where(e => e.UserId == userId).ExecuteDeleteAsync(cancellationToken)
            .ConfigureAwait(false);
        _ = await db.IncomeCategories.Where(e => e.UserId == userId).ExecuteDeleteAsync(cancellationToken)
            .ConfigureAwait(false);
        _ = await db.UserPreferences.Where(e => e.UserId == userId).ExecuteDeleteAsync(cancellationToken)
            .ConfigureAwait(false);
        _ = await db.PaymentInstruments.Where(e => e.UserId == userId).ExecuteDeleteAsync(cancellationToken)
            .ConfigureAwait(false);
        _ = await db.UserBookMetadata.Where(e => e.UserId == userId).ExecuteDeleteAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task SeedTaxonomyAsync(string userId, CancellationToken cancellationToken = default)
    {
        ValidateUserId(userId);
        DateTime utc = DateTime.UtcNow;
        bool hasCat = await db.Categories.AnyAsync(c => c.UserId == userId, cancellationToken).ConfigureAwait(false);
        if (hasCat)
        {
            throw new InvalidOperationException(
                "Taxonomy already exists for this user. Call reset-book first or use seed-demo.");
        }

        ExpenseTaxonomySeeder.Seed(db, userId, utc);
        IncomeTaxonomySeeder.Seed(db, userId, utc);
        _ = db.UserBookMetadata.Add(new UserBookMetadataEntity
        {
            UserId = userId,
            BookRevision = 0,
            UpdatedAtUtc = utc,
        });
        _ = await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task SeedDemoAsync(string userId, CancellationToken cancellationToken = default)
    {
        ValidateUserId(userId);
        await ResetUserBookAsync(userId, cancellationToken).ConfigureAwait(false);
        DateTime utc = DateTime.UtcNow;
        ExpenseTaxonomySeeder.Seed(db, userId, utc);
        IncomeTaxonomySeeder.Seed(db, userId, utc);

        _ = db.UserBookMetadata.Add(new UserBookMetadataEntity
        {
            UserId = userId,
            BookRevision = 0,
            UpdatedAtUtc = utc,
        });

        _ = db.PaymentInstruments.Add(new PaymentInstrumentEntity
        {
            UserId = userId,
            Id = "pi_demo_main",
            Label = "Demo Visa",
            BankName = "Demo Bank",
            IsActive = true,
            IsDefault = true,
            FeeDescription = "",
            UpdatedAtUtc = utc,
        });

        db.Expenses.AddRange(
            new ExpenseEntity
            {
                UserId = userId,
                Id = "exp_demo_rent_jan",
                OccurredOn = "2026-01-05",
                CategoryId = "cat_fixed_expenses",
                SubcategoryId = "cat_fixed_expenses_rent",
                AmountOriginal = 1200m,
                CurrencyCode = "USD",
                ManualFxRateToUsd = 1m,
                AmountUsd = 1200m,
                PaidWithCreditCard = false,
                Description = "Demo rent",
                UpdatedAtUtc = utc,
            },
            new ExpenseEntity
            {
                UserId = userId,
                Id = "exp_demo_fuel_feb",
                OccurredOn = "2026-02-12",
                CategoryId = "cat_transport",
                SubcategoryId = "cat_transport_fuel",
                AmountOriginal = 85.5m,
                CurrencyCode = "USD",
                ManualFxRateToUsd = 1m,
                AmountUsd = 85.5m,
                PaidWithCreditCard = true,
                PaymentInstrumentId = "pi_demo_main",
                Description = "Demo fuel",
                UpdatedAtUtc = utc,
            },
            new ExpenseEntity
            {
                UserId = userId,
                Id = "exp_demo_lunch_mar",
                OccurredOn = "2026-03-18",
                CategoryId = "cat_leisure",
                SubcategoryId = "cat_leisure_dining",
                AmountOriginal = 24m,
                CurrencyCode = "USD",
                ManualFxRateToUsd = 1m,
                AmountUsd = 24m,
                PaidWithCreditCard = true,
                PaymentInstrumentId = "pi_demo_main",
                Description = "Demo lunch",
                UpdatedAtUtc = utc,
            });

        db.IncomeEntries.AddRange(
            new IncomeEntryEntity
            {
                UserId = userId,
                Id = "inc_demo_salary_jan",
                ReceivedOn = "2026-01-01",
                IncomeCategoryId = "inc_cat_employment",
                IncomeSubcategoryId = "inc_sub_emp_salary",
                AmountOriginal = 5000m,
                CurrencyCode = "USD",
                ManualFxRateToUsd = 1m,
                AmountUsd = 5000m,
                Description = "Demo salary",
                UpdatedAtUtc = utc,
            },
            new IncomeEntryEntity
            {
                UserId = userId,
                Id = "inc_demo_bonus_feb",
                ReceivedOn = "2026-02-15",
                IncomeCategoryId = "inc_cat_employment",
                IncomeSubcategoryId = "inc_sub_emp_bonus",
                AmountOriginal = 800m,
                CurrencyCode = "USD",
                ManualFxRateToUsd = 1m,
                AmountUsd = 800m,
                Description = "Demo bonus",
                UpdatedAtUtc = utc,
            });

        _ = await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
