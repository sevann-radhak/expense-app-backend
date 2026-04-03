namespace ExpenseTracker.Infrastructure.Identity;

/// <summary>
/// Stable API/DB tier codes (see product doc <c>05-subscription-entitlements-strategy.md</c>).
/// </summary>
public static class SubscriptionTierCodes
{
    public const string Basic = "basic";

    public const string Pro = "pro";

    public const string ProMax = "pro_max";

    public static IReadOnlyList<string> All { get; } = [Basic, Pro, ProMax];

    public static bool IsValid(string? value)
    {
        return !string.IsNullOrWhiteSpace(value) && All.Contains(value, StringComparer.Ordinal);
    }
}

/// <summary>
/// How the current tier was assigned (billing integration later).
/// </summary>
public static class SubscriptionTierSourceCodes
{
    public const string Default = "default";

    public const string Admin = "admin";

    public const string Billing = "billing";

    public const string Promo = "promo";

    public static IReadOnlyList<string> All { get; } = [Default, Admin, Billing, Promo];

    public static bool IsValid(string? value)
    {
        return !string.IsNullOrWhiteSpace(value) && All.Contains(value, StringComparer.Ordinal);
    }
}
