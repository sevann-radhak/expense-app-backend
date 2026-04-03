using System.Text.Json;

namespace ExpenseTracker.Infrastructure.Sync;

public static class BookSyncValidator
{
    private const int MaxIdLength = 128;
    private const int MaxString512 = 512;

    public static string? ValidatePut(PutBookSyncRequest req)
    {
        if (req.SchemaVersion != BookSyncConstants.CurrentSchemaVersion)
        {
            return $"schemaVersion must be {BookSyncConstants.CurrentSchemaVersion}.";
        }

        List<CategorySyncDto> categories = req.Categories ?? [];
        List<SubcategorySyncDto> subcategories = req.Subcategories ?? [];
        List<IncomeCategorySyncDto> incomeCategories = req.IncomeCategories ?? [];
        List<IncomeSubcategorySyncDto> incomeSubcategories = req.IncomeSubcategories ?? [];
        List<PaymentInstrumentSyncDto> pis = req.PaymentInstruments ?? [];
        List<ExpenseRecurringSeriesSyncDto> ers = req.ExpenseRecurringSeries ?? [];
        List<IncomeRecurringSeriesSyncDto> irs = req.IncomeRecurringSeries ?? [];
        List<InstallmentPlanSyncDto> plans = req.InstallmentPlans ?? [];
        List<ExpenseSyncDto> expenses = req.Expenses ?? [];
        List<IncomeEntrySyncDto> incomeEntries = req.IncomeEntries ?? [];
        List<JsonElement> partials = req.PartialPayments ?? [];

        if (partials.Count > 0)
        {
            return "partialPayments must be empty (feature not implemented).";
        }

        string? err = ValidateCollectionIds("category", categories.Select(c => c.Id));
        if (err is not null)
        {
            return err;
        }

        err = ValidateCollectionIds("subcategory", subcategories.Select(s => s.Id));
        if (err is not null)
        {
            return err;
        }

        err = ValidateCollectionIds("incomeCategory", incomeCategories.Select(c => c.Id));
        if (err is not null)
        {
            return err;
        }

        err = ValidateCollectionIds("incomeSubcategory", incomeSubcategories.Select(s => s.Id));
        if (err is not null)
        {
            return err;
        }

        err = ValidateCollectionIds("paymentInstrument", pis.Select(p => p.Id));
        if (err is not null)
        {
            return err;
        }

        err = ValidateCollectionIds("expenseRecurringSeries", ers.Select(s => s.Id));
        if (err is not null)
        {
            return err;
        }

        err = ValidateCollectionIds("incomeRecurringSeries", irs.Select(s => s.Id));
        if (err is not null)
        {
            return err;
        }

        err = ValidateCollectionIds("installmentPlan", plans.Select(p => p.Id));
        if (err is not null)
        {
            return err;
        }

        err = ValidateCollectionIds("expense", expenses.Select(e => e.Id));
        if (err is not null)
        {
            return err;
        }

        err = ValidateCollectionIds("incomeEntry", incomeEntries.Select(e => e.Id));
        if (err is not null)
        {
            return err;
        }

        HashSet<string> catIds = new(categories.Select(c => c.Id), StringComparer.Ordinal);
        foreach (SubcategorySyncDto s in subcategories)
        {
            if (!catIds.Contains(s.CategoryId))
            {
                return $"Subcategory '{s.Id}' references unknown categoryId '{s.CategoryId}'.";
            }
        }

        HashSet<string> incCatIds = new(incomeCategories.Select(c => c.Id), StringComparer.Ordinal);
        foreach (IncomeSubcategorySyncDto s in incomeSubcategories)
        {
            if (!incCatIds.Contains(s.CategoryId))
            {
                return $"Income subcategory '{s.Id}' references unknown categoryId '{s.CategoryId}'.";
            }
        }

        HashSet<string> piIds = new(pis.Select(p => p.Id), StringComparer.Ordinal);
        foreach (ExpenseRecurringSeriesSyncDto s in ers)
        {
            if (!catIds.Contains(s.CategoryId) || !subcategories.Any(x => x.Id == s.SubcategoryId && x.CategoryId == s.CategoryId))
            {
                return $"Expense recurring series '{s.Id}' has invalid category/subcategory pair.";
            }

            if (s.PaymentInstrumentId is { Length: > 0 } pi && !piIds.Contains(pi))
            {
                return $"Expense recurring series '{s.Id}' references unknown paymentInstrumentId.";
            }

            if (s.Recurrence.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
            {
                return $"Expense recurring series '{s.Id}' must include recurrence.";
            }
        }

        foreach (IncomeRecurringSeriesSyncDto s in irs)
        {
            if (!incCatIds.Contains(s.IncomeCategoryId) ||
                !incomeSubcategories.Any(x => x.Id == s.IncomeSubcategoryId && x.CategoryId == s.IncomeCategoryId))
            {
                return $"Income recurring series '{s.Id}' has invalid income category/subcategory pair.";
            }

            if (s.Recurrence.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
            {
                return $"Income recurring series '{s.Id}' must include recurrence.";
            }
        }

        HashSet<string> planIds = new(plans.Select(p => p.Id), StringComparer.Ordinal);
        foreach (InstallmentPlanSyncDto p in plans)
        {
            if (!catIds.Contains(p.CategoryId) ||
                !subcategories.Any(x => x.Id == p.SubcategoryId && x.CategoryId == p.CategoryId))
            {
                return $"Installment plan '{p.Id}' has invalid category/subcategory pair.";
            }

            if (p.PaymentInstrumentId is { Length: > 0 } pi && !piIds.Contains(pi))
            {
                return $"Installment plan '{p.Id}' references unknown paymentInstrumentId.";
            }
        }

        HashSet<string> ersIds = new(ers.Select(s => s.Id), StringComparer.Ordinal);
        foreach (ExpenseSyncDto e in expenses)
        {
            if (!catIds.Contains(e.CategoryId) ||
                !subcategories.Any(x => x.Id == e.SubcategoryId && x.CategoryId == e.CategoryId))
            {
                return $"Expense '{e.Id}' has invalid category/subcategory pair.";
            }

            if (e.PaymentInstrumentId is { Length: > 0 } pi && !piIds.Contains(pi))
            {
                return $"Expense '{e.Id}' references unknown paymentInstrumentId.";
            }

            if (e.RecurringSeriesId is { Length: > 0 } rs && !ersIds.Contains(rs))
            {
                return $"Expense '{e.Id}' references unknown recurringSeriesId.";
            }

            if (e.InstallmentPlanId is { Length: > 0 } ip && !planIds.Contains(ip))
            {
                return $"Expense '{e.Id}' references unknown installmentPlanId.";
            }
        }

        HashSet<string> irsIds = new(irs.Select(s => s.Id), StringComparer.Ordinal);
        foreach (IncomeEntrySyncDto e in incomeEntries)
        {
            if (!incCatIds.Contains(e.IncomeCategoryId) ||
                !incomeSubcategories.Any(x => x.Id == e.IncomeSubcategoryId && x.CategoryId == e.IncomeCategoryId))
            {
                return $"Income entry '{e.Id}' has invalid income category/subcategory pair.";
            }

            if (e.RecurringSeriesId is { Length: > 0 } rs && !irsIds.Contains(rs))
            {
                return $"Income entry '{e.Id}' references unknown recurringSeriesId.";
            }
        }

        if (req.UserPreferences?.LastPaymentInstrumentId is { Length: > 0 } lastPi && !piIds.Contains(lastPi))
        {
            return "userPreferences.lastPaymentInstrumentId must reference a payment instrument in this snapshot.";
        }

        return ValidateStringLengths(req);
    }

    private static string? ValidateStringLengths(PutBookSyncRequest req)
    {
        foreach (CategorySyncDto c in req.Categories ?? [])
        {
            if (c.Name.Length > MaxString512)
            {
                return "category.name too long.";
            }
        }

        foreach (SubcategorySyncDto s in req.Subcategories ?? [])
        {
            if (s.Name.Length > MaxString512 || s.Slug.Length > 256)
            {
                return "subcategory name or slug too long.";
            }
        }

        return null;
    }

    private static string? ValidateCollectionIds(string label, IEnumerable<string> ids)
    {
        HashSet<string> seen = new(StringComparer.Ordinal);
        foreach (string id in ids)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return $"{label} id must be non-empty.";
            }

            if (id.Length > MaxIdLength)
            {
                return $"{label} id exceeds maximum length.";
            }

            if (!seen.Add(id))
            {
                return $"Duplicate {label} id '{id}'.";
            }
        }

        return null;
    }
}
