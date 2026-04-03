using ExpenseTracker.Infrastructure.Identity;

namespace ExpenseTracker.Infrastructure.Data.Entities;

public sealed class UserBookMetadataEntity
{
    public string UserId { get; set; } = null!;

    public ApplicationUser? User { get; set; }
    public int BookRevision { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

public sealed class UserPreferencesEntity
{
    public string UserId { get; set; } = null!;

    public ApplicationUser? User { get; set; }
    public string? Locale { get; set; }
    public string? DefaultCurrencyCode { get; set; }
    public string? LastPaymentInstrumentId { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

public sealed class PaymentInstrumentEntity
{
    public string UserId { get; set; } = null!;

    public ApplicationUser? User { get; set; }
    public string Id { get; set; } = null!;
    public string Label { get; set; } = null!;
    public string? BankName { get; set; }
    public int? BillingCycleDay { get; set; }
    public decimal? AnnualFeeAmount { get; set; }
    public decimal? MonthlyFeeAmount { get; set; }
    public string FeeDescription { get; set; } = "";
    public bool IsActive { get; set; } = true;
    public bool IsDefault { get; set; }
    public int? StatementClosingDay { get; set; }
    public int? PaymentDueDay { get; set; }
    public decimal? NominalAprPercent { get; set; }
    public decimal? CreditLimit { get; set; }
    public string? DisplaySuffix { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

public sealed class CategoryEntity
{
    public string UserId { get; set; } = null!;

    public ApplicationUser? User { get; set; }
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime UpdatedAtUtc { get; set; }
}

public sealed class SubcategoryEntity
{
    public string UserId { get; set; } = null!;

    public ApplicationUser? User { get; set; }
    public string Id { get; set; } = null!;
    public string CategoryId { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string Slug { get; set; } = null!;
    public bool IsSystemReserved { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime UpdatedAtUtc { get; set; }
}

public sealed class IncomeCategoryEntity
{
    public string UserId { get; set; } = null!;

    public ApplicationUser? User { get; set; }
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime UpdatedAtUtc { get; set; }
}

public sealed class IncomeSubcategoryEntity
{
    public string UserId { get; set; } = null!;

    public ApplicationUser? User { get; set; }
    public string Id { get; set; } = null!;
    public string CategoryId { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string Slug { get; set; } = null!;
    public bool IsSystemReserved { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime UpdatedAtUtc { get; set; }
}

public sealed class ExpenseRecurringSeriesEntity
{
    public string UserId { get; set; } = null!;

    public ApplicationUser? User { get; set; }
    public string Id { get; set; } = null!;
    public string AnchorOccurredOn { get; set; } = null!;
    public string RecurrenceJson { get; set; } = null!;
    public int HorizonMonths { get; set; }
    public bool Active { get; set; } = true;
    public string CategoryId { get; set; } = null!;
    public string SubcategoryId { get; set; } = null!;
    public decimal AmountOriginal { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public decimal ManualFxRateToUsd { get; set; } = 1m;
    public decimal AmountUsd { get; set; }
    public bool PaidWithCreditCard { get; set; }
    public string Description { get; set; } = "";
    public string? PaymentInstrumentId { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

public sealed class IncomeRecurringSeriesEntity
{
    public string UserId { get; set; } = null!;

    public ApplicationUser? User { get; set; }
    public string Id { get; set; } = null!;
    public string AnchorReceivedOn { get; set; } = null!;
    public string RecurrenceJson { get; set; } = null!;
    public int HorizonMonths { get; set; }
    public bool Active { get; set; } = true;
    public string IncomeCategoryId { get; set; } = null!;
    public string IncomeSubcategoryId { get; set; } = null!;
    public decimal AmountOriginal { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public decimal ManualFxRateToUsd { get; set; } = 1m;
    public decimal AmountUsd { get; set; }
    public string Description { get; set; } = "";
    public DateTime UpdatedAtUtc { get; set; }
}

public sealed class InstallmentPlanEntity
{
    public string UserId { get; set; } = null!;

    public ApplicationUser? User { get; set; }
    public string Id { get; set; } = null!;
    public int PaymentCount { get; set; }
    public int IntervalMonths { get; set; } = 1;
    public string AnchorOccurredOn { get; set; } = null!;
    public string CategoryId { get; set; } = null!;
    public string SubcategoryId { get; set; } = null!;
    public string? PaymentInstrumentId { get; set; }
    public decimal PerPaymentAmountOriginal { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public decimal ManualFxRateToUsd { get; set; } = 1m;
    public decimal PerPaymentAmountUsd { get; set; }
    public string Description { get; set; } = "";
    public DateTime UpdatedAtUtc { get; set; }
}

public sealed class ExpenseEntity
{
    public string UserId { get; set; } = null!;

    public ApplicationUser? User { get; set; }
    public string Id { get; set; } = null!;
    public string OccurredOn { get; set; } = null!;
    public string CategoryId { get; set; } = null!;
    public string SubcategoryId { get; set; } = null!;
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
    public DateTime UpdatedAtUtc { get; set; }
}

public sealed class IncomeEntryEntity
{
    public string UserId { get; set; } = null!;

    public ApplicationUser? User { get; set; }
    public string Id { get; set; } = null!;
    public string ReceivedOn { get; set; } = null!;
    public string IncomeCategoryId { get; set; } = null!;
    public string IncomeSubcategoryId { get; set; } = null!;
    public decimal AmountOriginal { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public decimal ManualFxRateToUsd { get; set; } = 1m;
    public decimal AmountUsd { get; set; }
    public string Description { get; set; } = "";
    public string? RecurringSeriesId { get; set; }
    public string? ExpectationStatus { get; set; }
    public string? ExpectationConfirmedOn { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}
