using System.Text.Json;
using System.Text.Json.Serialization;

namespace ExpenseTracker.Infrastructure.Sync;

public sealed class BookSyncGetResponse
{
    [JsonPropertyName("bookRevision")]
    public int BookRevision { get; set; }

    [JsonPropertyName("schemaVersion")]
    public int SchemaVersion { get; set; }

    [JsonPropertyName("exportedAt")]
    public DateTime ExportedAt { get; set; }

    [JsonPropertyName("categories")]
    public List<CategorySyncDto> Categories { get; set; } = [];

    [JsonPropertyName("subcategories")]
    public List<SubcategorySyncDto> Subcategories { get; set; } = [];

    [JsonPropertyName("incomeCategories")]
    public List<IncomeCategorySyncDto> IncomeCategories { get; set; } = [];

    [JsonPropertyName("incomeSubcategories")]
    public List<IncomeSubcategorySyncDto> IncomeSubcategories { get; set; } = [];

    [JsonPropertyName("paymentInstruments")]
    public List<PaymentInstrumentSyncDto> PaymentInstruments { get; set; } = [];

    [JsonPropertyName("expenseRecurringSeries")]
    public List<ExpenseRecurringSeriesSyncDto> ExpenseRecurringSeries { get; set; } = [];

    [JsonPropertyName("expenses")]
    public List<ExpenseSyncDto> Expenses { get; set; } = [];

    [JsonPropertyName("incomeEntries")]
    public List<IncomeEntrySyncDto> IncomeEntries { get; set; } = [];

    [JsonPropertyName("incomeRecurringSeries")]
    public List<IncomeRecurringSeriesSyncDto> IncomeRecurringSeries { get; set; } = [];

    [JsonPropertyName("installmentPlans")]
    public List<InstallmentPlanSyncDto> InstallmentPlans { get; set; } = [];

    [JsonPropertyName("partialPayments")]
    public List<JsonElement> PartialPayments { get; set; } = [];

    [JsonPropertyName("userPreferences")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public UserPreferencesSyncDto? UserPreferences { get; set; }
}

public sealed class PutBookSyncRequest
{
    [JsonPropertyName("expectedBookRevision")]
    public int ExpectedBookRevision { get; set; }

    [JsonPropertyName("schemaVersion")]
    public int SchemaVersion { get; set; }

    [JsonPropertyName("exportedAt")]
    public DateTime ExportedAt { get; set; }

    [JsonPropertyName("categories")]
    public List<CategorySyncDto>? Categories { get; set; }

    [JsonPropertyName("subcategories")]
    public List<SubcategorySyncDto>? Subcategories { get; set; }

    [JsonPropertyName("incomeCategories")]
    public List<IncomeCategorySyncDto>? IncomeCategories { get; set; }

    [JsonPropertyName("incomeSubcategories")]
    public List<IncomeSubcategorySyncDto>? IncomeSubcategories { get; set; }

    [JsonPropertyName("paymentInstruments")]
    public List<PaymentInstrumentSyncDto>? PaymentInstruments { get; set; }

    [JsonPropertyName("expenseRecurringSeries")]
    public List<ExpenseRecurringSeriesSyncDto>? ExpenseRecurringSeries { get; set; }

    [JsonPropertyName("expenses")]
    public List<ExpenseSyncDto>? Expenses { get; set; }

    [JsonPropertyName("incomeEntries")]
    public List<IncomeEntrySyncDto>? IncomeEntries { get; set; }

    [JsonPropertyName("incomeRecurringSeries")]
    public List<IncomeRecurringSeriesSyncDto>? IncomeRecurringSeries { get; set; }

    [JsonPropertyName("installmentPlans")]
    public List<InstallmentPlanSyncDto>? InstallmentPlans { get; set; }

    [JsonPropertyName("partialPayments")]
    public List<JsonElement>? PartialPayments { get; set; }

    [JsonPropertyName("userPreferences")]
    public UserPreferencesSyncDto? UserPreferences { get; set; }
}

public sealed class UserPreferencesSyncDto
{
    [JsonPropertyName("locale")]
    public string? Locale { get; set; }

    [JsonPropertyName("defaultCurrencyCode")]
    public string? DefaultCurrencyCode { get; set; }

    [JsonPropertyName("lastPaymentInstrumentId")]
    public string? LastPaymentInstrumentId { get; set; }
}

public sealed class CategorySyncDto
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class SubcategorySyncDto
{
    public string Id { get; set; } = "";
    public string CategoryId { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public string Slug { get; set; } = "";
    public bool IsSystemReserved { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class IncomeCategorySyncDto
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class IncomeSubcategorySyncDto
{
    public string Id { get; set; } = "";
    public string CategoryId { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public string Slug { get; set; } = "";
    public bool IsSystemReserved { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class PaymentInstrumentSyncDto
{
    public string Id { get; set; } = "";
    public string Label { get; set; } = "";
    public string? BankName { get; set; }
    public int? BillingCycleDay { get; set; }
    public decimal? AnnualFeeAmount { get; set; }
    public decimal? MonthlyFeeAmount { get; set; }
    public string? FeeDescription { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDefault { get; set; }
    public int? StatementClosingDay { get; set; }
    public int? PaymentDueDay { get; set; }
    public decimal? NominalAprPercent { get; set; }
    public decimal? CreditLimit { get; set; }
    public string? DisplaySuffix { get; set; }
}

public sealed class ExpenseRecurringSeriesSyncDto
{
    public string Id { get; set; } = "";
    public string AnchorOccurredOn { get; set; } = "";
    public JsonElement Recurrence { get; set; }
    public int HorizonMonths { get; set; }
    public bool Active { get; set; } = true;
    public string CategoryId { get; set; } = "";
    public string SubcategoryId { get; set; } = "";
    public decimal AmountOriginal { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public decimal ManualFxRateToUsd { get; set; } = 1m;
    public decimal AmountUsd { get; set; }
    public bool PaidWithCreditCard { get; set; }
    public string Description { get; set; } = "";
    public string? PaymentInstrumentId { get; set; }
}

public sealed class IncomeRecurringSeriesSyncDto
{
    public string Id { get; set; } = "";
    public string AnchorReceivedOn { get; set; } = "";
    public JsonElement Recurrence { get; set; }
    public int HorizonMonths { get; set; }
    public bool Active { get; set; } = true;
    public string IncomeCategoryId { get; set; } = "";
    public string IncomeSubcategoryId { get; set; } = "";
    public decimal AmountOriginal { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public decimal ManualFxRateToUsd { get; set; } = 1m;
    public decimal AmountUsd { get; set; }
    public string Description { get; set; } = "";
}

public sealed class InstallmentPlanSyncDto
{
    public string Id { get; set; } = "";
    public int PaymentCount { get; set; }
    public int IntervalMonths { get; set; } = 1;
    public string AnchorOccurredOn { get; set; } = "";
    public string CategoryId { get; set; } = "";
    public string SubcategoryId { get; set; } = "";
    public string? PaymentInstrumentId { get; set; }
    public decimal PerPaymentAmountOriginal { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public decimal ManualFxRateToUsd { get; set; } = 1m;
    public decimal PerPaymentAmountUsd { get; set; }
    public string Description { get; set; } = "";
}

public sealed class ExpenseSyncDto
{
    public string Id { get; set; } = "";
    public string OccurredOn { get; set; } = "";
    public string CategoryId { get; set; } = "";
    public string SubcategoryId { get; set; } = "";
    public decimal AmountOriginal { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public decimal ManualFxRateToUsd { get; set; } = 1m;
    public decimal AmountUsd { get; set; }
    public bool PaidWithCreditCard { get; set; }
    public string Description { get; set; } = "";
    public string? PaymentInstrumentId { get; set; }
    public string? RecurringSeriesId { get; set; }
    public string? PaymentExpectationStatus { get; set; }
    public string? PaymentExpectationConfirmedOn { get; set; }
    public string? InstallmentPlanId { get; set; }
    public int? InstallmentIndex { get; set; }
}

public sealed class IncomeEntrySyncDto
{
    public string Id { get; set; } = "";
    public string ReceivedOn { get; set; } = "";
    public string IncomeCategoryId { get; set; } = "";
    public string IncomeSubcategoryId { get; set; } = "";
    public decimal AmountOriginal { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public decimal ManualFxRateToUsd { get; set; } = 1m;
    public decimal AmountUsd { get; set; }
    public string Description { get; set; } = "";
    public string? RecurringSeriesId { get; set; }
    public string? ExpectationStatus { get; set; }
    public string? ExpectationConfirmedOn { get; set; }
}
