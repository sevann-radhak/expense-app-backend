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
        bool userExists = await db.Users.AnyAsync(u => u.Id == userId, cancellationToken).ConfigureAwait(false);
        if (!userExists)
        {
            throw new DevBookUserNotFoundException(userId);
        }

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
        await SeedTaxonomyAsync(userId, cancellationToken).ConfigureAwait(false);

        DateTime utc = DateTime.UtcNow;
        var paymentInstruments = new List<PaymentInstrumentEntity>();
        var expenses = new List<ExpenseEntity>();
        var incomeEntries = new List<IncomeEntryEntity>();
        DevDemoScenarioGenerator.AppendDemoRows(userId, utc, paymentInstruments, expenses, incomeEntries);
        db.PaymentInstruments.AddRange(paymentInstruments);
        db.Expenses.AddRange(expenses);
        db.IncomeEntries.AddRange(incomeEntries);

        _ = await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
