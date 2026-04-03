using ExpenseTracker.Infrastructure.Data;
using ExpenseTracker.Infrastructure.Data.Entities;
using ExpenseTracker.Infrastructure.Sync;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;
using System.Text.Json;

namespace ExpenseTracker.Infrastructure.Services;

public sealed class BookSyncService(ExpenseTrackerDbContext db, DevBookDataService devBook)
{
    public async Task<BookSyncGetResponse> GetAsync(string userId, CancellationToken cancellationToken = default)
    {
        DevBookDataService.ValidateUserId(userId);
        UserBookMetadataEntity? meta = await db.UserBookMetadata.AsNoTracking()
            .FirstOrDefaultAsync(m => m.UserId == userId, cancellationToken).ConfigureAwait(false);
        int revision = meta?.BookRevision ?? 0;
        DateTime exportedAt = meta?.UpdatedAtUtc ?? DateTime.UtcNow;

        List<CategoryEntity> categories = await db.Categories.AsNoTracking()
            .Where(c => c.UserId == userId).OrderBy(c => c.SortOrder).ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        List<SubcategoryEntity> subcategories = await db.Subcategories.AsNoTracking()
            .Where(c => c.UserId == userId).OrderBy(c => c.SortOrder).ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        List<IncomeCategoryEntity> incomeCategories = await db.IncomeCategories.AsNoTracking()
            .Where(c => c.UserId == userId).OrderBy(c => c.SortOrder).ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        List<IncomeSubcategoryEntity> incomeSubcategories = await db.IncomeSubcategories.AsNoTracking()
            .Where(c => c.UserId == userId).OrderBy(c => c.SortOrder).ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        List<PaymentInstrumentEntity> pis = await db.PaymentInstruments.AsNoTracking()
            .Where(c => c.UserId == userId).ToListAsync(cancellationToken).ConfigureAwait(false);
        List<ExpenseRecurringSeriesEntity> ers = await db.ExpenseRecurringSeries.AsNoTracking()
            .Where(c => c.UserId == userId).ToListAsync(cancellationToken).ConfigureAwait(false);
        List<IncomeRecurringSeriesEntity> irs = await db.IncomeRecurringSeries.AsNoTracking()
            .Where(c => c.UserId == userId).ToListAsync(cancellationToken).ConfigureAwait(false);
        List<InstallmentPlanEntity> plans = await db.InstallmentPlans.AsNoTracking()
            .Where(c => c.UserId == userId).ToListAsync(cancellationToken).ConfigureAwait(false);
        List<ExpenseEntity> expenses = await db.Expenses.AsNoTracking()
            .Where(c => c.UserId == userId).ToListAsync(cancellationToken).ConfigureAwait(false);
        List<IncomeEntryEntity> incomeEntries = await db.IncomeEntries.AsNoTracking()
            .Where(c => c.UserId == userId).ToListAsync(cancellationToken).ConfigureAwait(false);
        UserPreferencesEntity? prefs = await db.UserPreferences.AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken).ConfigureAwait(false);

        return new BookSyncGetResponse
        {
            BookRevision = revision,
            SchemaVersion = BookSyncConstants.CurrentSchemaVersion,
            ExportedAt = exportedAt,
            Categories = categories.Select(MapCategory).ToList(),
            Subcategories = subcategories.Select(MapSubcategory).ToList(),
            IncomeCategories = incomeCategories.Select(MapIncomeCategory).ToList(),
            IncomeSubcategories = incomeSubcategories.Select(MapIncomeSubcategory).ToList(),
            PaymentInstruments = pis.Select(MapPi).ToList(),
            ExpenseRecurringSeries = ers.Select(MapErs).ToList(),
            Expenses = expenses.Select(MapExpense).ToList(),
            IncomeEntries = incomeEntries.Select(MapIncomeEntry).ToList(),
            IncomeRecurringSeries = irs.Select(MapIrs).ToList(),
            InstallmentPlans = plans.Select(MapPlan).ToList(),
            PartialPayments = [],
            UserPreferences = prefs is null
                ? null
                : new UserPreferencesSyncDto
                {
                    Locale = prefs.Locale,
                    DefaultCurrencyCode = prefs.DefaultCurrencyCode,
                    LastPaymentInstrumentId = prefs.LastPaymentInstrumentId,
                },
        };
    }

    public async Task<BookSyncReplaceResult> TryReplaceAsync(
        string userId,
        PutBookSyncRequest request,
        CancellationToken cancellationToken = default)
    {
        DevBookDataService.ValidateUserId(userId);
        string? validation = BookSyncValidator.ValidatePut(request);
        if (validation is not null)
        {
            return BookSyncReplaceResult.BadRequest(validation);
        }

        await using IDbContextTransaction tx =
            await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken)
                .ConfigureAwait(false);
        try
        {
            UserBookMetadataEntity? meta = await db.UserBookMetadata
                .FirstOrDefaultAsync(m => m.UserId == userId, cancellationToken).ConfigureAwait(false);
            int currentRevision = meta?.BookRevision ?? 0;
            if (currentRevision != request.ExpectedBookRevision)
            {
                await tx.RollbackAsync(cancellationToken).ConfigureAwait(false);
                return BookSyncReplaceResult.RevisionConflict(currentRevision);
            }

            await devBook.ResetUserBookAsync(userId, cancellationToken).ConfigureAwait(false);

            DateTime utc = DateTime.UtcNow;
            int nextRevision = currentRevision + 1;

            foreach (PaymentInstrumentSyncDto d in request.PaymentInstruments ?? [])
            {
                _ = db.PaymentInstruments.Add(MapPiEntity(userId, d, utc));
            }

            foreach (CategorySyncDto d in request.Categories ?? [])
            {
                _ = db.Categories.Add(MapCategoryEntity(userId, d, utc));
            }

            foreach (SubcategorySyncDto d in request.Subcategories ?? [])
            {
                _ = db.Subcategories.Add(MapSubcategoryEntity(userId, d, utc));
            }

            foreach (IncomeCategorySyncDto d in request.IncomeCategories ?? [])
            {
                _ = db.IncomeCategories.Add(MapIncomeCategoryEntity(userId, d, utc));
            }

            foreach (IncomeSubcategorySyncDto d in request.IncomeSubcategories ?? [])
            {
                _ = db.IncomeSubcategories.Add(MapIncomeSubcategoryEntity(userId, d, utc));
            }

            foreach (ExpenseRecurringSeriesSyncDto d in request.ExpenseRecurringSeries ?? [])
            {
                _ = db.ExpenseRecurringSeries.Add(MapErsEntity(userId, d, utc));
            }

            foreach (IncomeRecurringSeriesSyncDto d in request.IncomeRecurringSeries ?? [])
            {
                _ = db.IncomeRecurringSeries.Add(MapIrsEntity(userId, d, utc));
            }

            foreach (InstallmentPlanSyncDto d in request.InstallmentPlans ?? [])
            {
                _ = db.InstallmentPlans.Add(MapPlanEntity(userId, d, utc));
            }

            foreach (ExpenseSyncDto d in request.Expenses ?? [])
            {
                _ = db.Expenses.Add(MapExpenseEntity(userId, d, utc));
            }

            foreach (IncomeEntrySyncDto d in request.IncomeEntries ?? [])
            {
                _ = db.IncomeEntries.Add(MapIncomeEntryEntity(userId, d, utc));
            }

            if (request.UserPreferences is not null)
            {
                _ = db.UserPreferences.Add(new UserPreferencesEntity
                {
                    UserId = userId,
                    Locale = request.UserPreferences.Locale,
                    DefaultCurrencyCode = request.UserPreferences.DefaultCurrencyCode,
                    LastPaymentInstrumentId = request.UserPreferences.LastPaymentInstrumentId,
                    UpdatedAtUtc = utc,
                });
            }

            _ = db.UserBookMetadata.Add(new UserBookMetadataEntity
            {
                UserId = userId,
                BookRevision = nextRevision,
                UpdatedAtUtc = utc,
            });

            _ = await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            await tx.CommitAsync(cancellationToken).ConfigureAwait(false);
            return BookSyncReplaceResult.Ok(nextRevision);
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken).ConfigureAwait(false);
            throw;
        }
    }

    private static CategorySyncDto MapCategory(CategoryEntity e)
    {
        return new()
        {
            Id = e.Id,
            Name = e.Name,
            Description = e.Description,
            SortOrder = e.SortOrder,
            IsActive = e.IsActive,
        };
    }

    private static SubcategorySyncDto MapSubcategory(SubcategoryEntity e)
    {
        return new()
        {
            Id = e.Id,
            CategoryId = e.CategoryId,
            Name = e.Name,
            Description = e.Description,
            Slug = e.Slug,
            IsSystemReserved = e.IsSystemReserved,
            SortOrder = e.SortOrder,
            IsActive = e.IsActive,
        };
    }

    private static IncomeCategorySyncDto MapIncomeCategory(IncomeCategoryEntity e)
    {
        return new()
        {
            Id = e.Id,
            Name = e.Name,
            Description = e.Description,
            SortOrder = e.SortOrder,
            IsActive = e.IsActive,
        };
    }

    private static IncomeSubcategorySyncDto MapIncomeSubcategory(IncomeSubcategoryEntity e)
    {
        return new()
        {
            Id = e.Id,
            CategoryId = e.CategoryId,
            Name = e.Name,
            Description = e.Description,
            Slug = e.Slug,
            IsSystemReserved = e.IsSystemReserved,
            SortOrder = e.SortOrder,
            IsActive = e.IsActive,
        };
    }

    private static PaymentInstrumentSyncDto MapPi(PaymentInstrumentEntity e)
    {
        return new()
        {
            Id = e.Id,
            Label = e.Label,
            BankName = e.BankName,
            BillingCycleDay = e.BillingCycleDay,
            AnnualFeeAmount = e.AnnualFeeAmount,
            MonthlyFeeAmount = e.MonthlyFeeAmount,
            FeeDescription = e.FeeDescription,
            IsActive = e.IsActive,
            IsDefault = e.IsDefault,
            StatementClosingDay = e.StatementClosingDay,
            PaymentDueDay = e.PaymentDueDay,
            NominalAprPercent = e.NominalAprPercent,
            CreditLimit = e.CreditLimit,
            DisplaySuffix = e.DisplaySuffix,
        };
    }

    private static ExpenseRecurringSeriesSyncDto MapErs(ExpenseRecurringSeriesEntity e)
    {
        return new()
        {
            Id = e.Id,
            AnchorOccurredOn = e.AnchorOccurredOn,
            Recurrence = ParseRecurrenceJson(e.RecurrenceJson),
            HorizonMonths = e.HorizonMonths,
            Active = e.Active,
            CategoryId = e.CategoryId,
            SubcategoryId = e.SubcategoryId,
            AmountOriginal = e.AmountOriginal,
            CurrencyCode = e.CurrencyCode,
            ManualFxRateToUsd = e.ManualFxRateToUsd,
            AmountUsd = e.AmountUsd,
            PaidWithCreditCard = e.PaidWithCreditCard,
            Description = e.Description,
            PaymentInstrumentId = e.PaymentInstrumentId,
        };
    }

    private static IncomeRecurringSeriesSyncDto MapIrs(IncomeRecurringSeriesEntity e)
    {
        return new()
        {
            Id = e.Id,
            AnchorReceivedOn = e.AnchorReceivedOn,
            Recurrence = ParseRecurrenceJson(e.RecurrenceJson),
            HorizonMonths = e.HorizonMonths,
            Active = e.Active,
            IncomeCategoryId = e.IncomeCategoryId,
            IncomeSubcategoryId = e.IncomeSubcategoryId,
            AmountOriginal = e.AmountOriginal,
            CurrencyCode = e.CurrencyCode,
            ManualFxRateToUsd = e.ManualFxRateToUsd,
            AmountUsd = e.AmountUsd,
            Description = e.Description,
        };
    }

    private static InstallmentPlanSyncDto MapPlan(InstallmentPlanEntity e)
    {
        return new()
        {
            Id = e.Id,
            PaymentCount = e.PaymentCount,
            IntervalMonths = e.IntervalMonths,
            AnchorOccurredOn = e.AnchorOccurredOn,
            CategoryId = e.CategoryId,
            SubcategoryId = e.SubcategoryId,
            PaymentInstrumentId = e.PaymentInstrumentId,
            PerPaymentAmountOriginal = e.PerPaymentAmountOriginal,
            CurrencyCode = e.CurrencyCode,
            ManualFxRateToUsd = e.ManualFxRateToUsd,
            PerPaymentAmountUsd = e.PerPaymentAmountUsd,
            Description = e.Description,
        };
    }

    private static ExpenseSyncDto MapExpense(ExpenseEntity e)
    {
        ExpenseSyncDto d = new()
        {
            Id = e.Id,
            OccurredOn = e.OccurredOn,
            CategoryId = e.CategoryId,
            SubcategoryId = e.SubcategoryId,
            AmountOriginal = e.AmountOriginal,
            CurrencyCode = e.CurrencyCode,
            ManualFxRateToUsd = e.ManualFxRateToUsd,
            AmountUsd = e.AmountUsd,
            PaidWithCreditCard = e.PaidWithCreditCard,
            Description = e.Description,
            PaymentInstrumentId = e.PaymentInstrumentId,
            RecurringSeriesId = e.RecurringSeriesId,
            PaymentExpectationStatus = e.PaymentExpectationStatus,
            PaymentExpectationConfirmedOn = e.PaymentExpectationConfirmedOn,
            InstallmentPlanId = e.InstallmentPlanId,
            InstallmentIndex = e.InstallmentIndex,
        };
        return d;
    }

    private static IncomeEntrySyncDto MapIncomeEntry(IncomeEntryEntity e)
    {
        return new()
        {
            Id = e.Id,
            ReceivedOn = e.ReceivedOn,
            IncomeCategoryId = e.IncomeCategoryId,
            IncomeSubcategoryId = e.IncomeSubcategoryId,
            AmountOriginal = e.AmountOriginal,
            CurrencyCode = e.CurrencyCode,
            ManualFxRateToUsd = e.ManualFxRateToUsd,
            AmountUsd = e.AmountUsd,
            Description = e.Description,
            RecurringSeriesId = e.RecurringSeriesId,
            ExpectationStatus = e.ExpectationStatus,
            ExpectationConfirmedOn = e.ExpectationConfirmedOn,
        };
    }

    private static JsonElement ParseRecurrenceJson(string json)
    {
        try
        {
            using JsonDocument doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(json) ? "{}" : json);
            return doc.RootElement.Clone();
        }
        catch (JsonException)
        {
            return JsonDocument.Parse("{}").RootElement.Clone();
        }
    }

    private static PaymentInstrumentEntity MapPiEntity(string userId, PaymentInstrumentSyncDto d, DateTime utc)
    {
        return new()
        {
            UserId = userId,
            Id = d.Id,
            Label = d.Label,
            BankName = d.BankName,
            BillingCycleDay = d.BillingCycleDay,
            AnnualFeeAmount = d.AnnualFeeAmount,
            MonthlyFeeAmount = d.MonthlyFeeAmount,
            FeeDescription = d.FeeDescription ?? string.Empty,
            IsActive = d.IsActive,
            IsDefault = d.IsDefault,
            StatementClosingDay = d.StatementClosingDay,
            PaymentDueDay = d.PaymentDueDay,
            NominalAprPercent = d.NominalAprPercent,
            CreditLimit = d.CreditLimit,
            DisplaySuffix = d.DisplaySuffix,
            UpdatedAtUtc = utc,
        };
    }

    private static CategoryEntity MapCategoryEntity(string userId, CategorySyncDto d, DateTime utc)
    {
        return new()
        {
            UserId = userId,
            Id = d.Id,
            Name = d.Name,
            Description = d.Description,
            SortOrder = d.SortOrder,
            IsActive = d.IsActive,
            UpdatedAtUtc = utc,
        };
    }

    private static SubcategoryEntity MapSubcategoryEntity(string userId, SubcategorySyncDto d, DateTime utc)
    {
        return new()
        {
            UserId = userId,
            Id = d.Id,
            CategoryId = d.CategoryId,
            Name = d.Name,
            Description = d.Description,
            Slug = d.Slug,
            IsSystemReserved = d.IsSystemReserved,
            SortOrder = d.SortOrder,
            IsActive = d.IsActive,
            UpdatedAtUtc = utc,
        };
    }

    private static IncomeCategoryEntity MapIncomeCategoryEntity(string userId, IncomeCategorySyncDto d, DateTime utc)
    {
        return new()
        {
            UserId = userId,
            Id = d.Id,
            Name = d.Name,
            Description = d.Description,
            SortOrder = d.SortOrder,
            IsActive = d.IsActive,
            UpdatedAtUtc = utc,
        };
    }

    private static IncomeSubcategoryEntity MapIncomeSubcategoryEntity(
        string userId,
        IncomeSubcategorySyncDto d,
        DateTime utc)
    {
        return new()
        {
            UserId = userId,
            Id = d.Id,
            CategoryId = d.CategoryId,
            Name = d.Name,
            Description = d.Description,
            Slug = d.Slug,
            IsSystemReserved = d.IsSystemReserved,
            SortOrder = d.SortOrder,
            IsActive = d.IsActive,
            UpdatedAtUtc = utc,
        };
    }

    private static ExpenseRecurringSeriesEntity MapErsEntity(string userId, ExpenseRecurringSeriesSyncDto d, DateTime utc)
    {
        return new()
        {
            UserId = userId,
            Id = d.Id,
            AnchorOccurredOn = d.AnchorOccurredOn,
            RecurrenceJson = d.Recurrence.GetRawText(),
            HorizonMonths = d.HorizonMonths,
            Active = d.Active,
            CategoryId = d.CategoryId,
            SubcategoryId = d.SubcategoryId,
            AmountOriginal = d.AmountOriginal,
            CurrencyCode = d.CurrencyCode,
            ManualFxRateToUsd = d.ManualFxRateToUsd,
            AmountUsd = d.AmountUsd,
            PaidWithCreditCard = d.PaidWithCreditCard,
            Description = d.Description,
            PaymentInstrumentId = string.IsNullOrEmpty(d.PaymentInstrumentId) ? null : d.PaymentInstrumentId,
            UpdatedAtUtc = utc,
        };
    }

    private static IncomeRecurringSeriesEntity MapIrsEntity(string userId, IncomeRecurringSeriesSyncDto d, DateTime utc)
    {
        return new()
        {
            UserId = userId,
            Id = d.Id,
            AnchorReceivedOn = d.AnchorReceivedOn,
            RecurrenceJson = d.Recurrence.GetRawText(),
            HorizonMonths = d.HorizonMonths,
            Active = d.Active,
            IncomeCategoryId = d.IncomeCategoryId,
            IncomeSubcategoryId = d.IncomeSubcategoryId,
            AmountOriginal = d.AmountOriginal,
            CurrencyCode = d.CurrencyCode,
            ManualFxRateToUsd = d.ManualFxRateToUsd,
            AmountUsd = d.AmountUsd,
            Description = d.Description,
            UpdatedAtUtc = utc,
        };
    }

    private static InstallmentPlanEntity MapPlanEntity(string userId, InstallmentPlanSyncDto d, DateTime utc)
    {
        int interval = d.IntervalMonths < 1 ? 1 : d.IntervalMonths;
        return new InstallmentPlanEntity
        {
            UserId = userId,
            Id = d.Id,
            PaymentCount = d.PaymentCount,
            IntervalMonths = interval,
            AnchorOccurredOn = d.AnchorOccurredOn,
            CategoryId = d.CategoryId,
            SubcategoryId = d.SubcategoryId,
            PaymentInstrumentId = string.IsNullOrEmpty(d.PaymentInstrumentId) ? null : d.PaymentInstrumentId,
            PerPaymentAmountOriginal = d.PerPaymentAmountOriginal,
            CurrencyCode = d.CurrencyCode,
            ManualFxRateToUsd = d.ManualFxRateToUsd,
            PerPaymentAmountUsd = d.PerPaymentAmountUsd,
            Description = d.Description,
            UpdatedAtUtc = utc,
        };
    }

    private static ExpenseEntity MapExpenseEntity(string userId, ExpenseSyncDto d, DateTime utc)
    {
        return new()
        {
            UserId = userId,
            Id = d.Id,
            OccurredOn = d.OccurredOn,
            CategoryId = d.CategoryId,
            SubcategoryId = d.SubcategoryId,
            AmountOriginal = d.AmountOriginal,
            CurrencyCode = d.CurrencyCode,
            ManualFxRateToUsd = d.ManualFxRateToUsd,
            AmountUsd = d.AmountUsd,
            PaidWithCreditCard = d.PaidWithCreditCard,
            Description = d.Description,
            PaymentInstrumentId = string.IsNullOrEmpty(d.PaymentInstrumentId) ? null : d.PaymentInstrumentId,
            RecurringSeriesId = string.IsNullOrEmpty(d.RecurringSeriesId) ? null : d.RecurringSeriesId,
            PaymentExpectationStatus = d.PaymentExpectationStatus,
            PaymentExpectationConfirmedOn = d.PaymentExpectationConfirmedOn,
            InstallmentPlanId = string.IsNullOrEmpty(d.InstallmentPlanId) ? null : d.InstallmentPlanId,
            InstallmentIndex = d.InstallmentIndex,
            UpdatedAtUtc = utc,
        };
    }

    private static IncomeEntryEntity MapIncomeEntryEntity(string userId, IncomeEntrySyncDto d, DateTime utc)
    {
        return new()
        {
            UserId = userId,
            Id = d.Id,
            ReceivedOn = d.ReceivedOn,
            IncomeCategoryId = d.IncomeCategoryId,
            IncomeSubcategoryId = d.IncomeSubcategoryId,
            AmountOriginal = d.AmountOriginal,
            CurrencyCode = d.CurrencyCode,
            ManualFxRateToUsd = d.ManualFxRateToUsd,
            AmountUsd = d.AmountUsd,
            Description = d.Description,
            RecurringSeriesId = string.IsNullOrEmpty(d.RecurringSeriesId) ? null : d.RecurringSeriesId,
            ExpectationStatus = d.ExpectationStatus,
            ExpectationConfirmedOn = d.ExpectationConfirmedOn,
            UpdatedAtUtc = utc,
        };
    }
}

public sealed class BookSyncReplaceResult
{
    private BookSyncReplaceResult(bool success, bool isConflict, int newRevision, int currentRevision, string? badRequest)
    {
        Success = success;
        IsConflict = isConflict;
        NewBookRevision = newRevision;
        CurrentBookRevision = currentRevision;
        BadRequestDetail = badRequest;
    }

    public bool Success { get; }
    public bool IsConflict { get; }
    public int NewBookRevision { get; }
    public int CurrentBookRevision { get; }
    public string? BadRequestDetail { get; }

    public static BookSyncReplaceResult Ok(int newRevision)
    {
        return new(true, false, newRevision, 0, null);
    }

    public static BookSyncReplaceResult RevisionConflict(int currentRevision)
    {
        return new(false, true, 0, currentRevision, null);
    }

    public static BookSyncReplaceResult BadRequest(string detail)
    {
        return new(false, false, 0, 0, detail);
    }
}
