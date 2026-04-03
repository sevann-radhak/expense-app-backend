using Microsoft.AspNetCore.Identity;

namespace ExpenseTracker.Infrastructure.Identity;

public sealed class ApplicationUser : IdentityUser
{
    public string? DisplayName { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    /// <summary>Product tier: <see cref="SubscriptionTierCodes"/>.</summary>
    public string SubscriptionTier { get; set; } = SubscriptionTierCodes.Basic;

    /// <summary>Assignment source: <see cref="SubscriptionTierSourceCodes"/>.</summary>
    public string SubscriptionTierSource { get; set; } = SubscriptionTierSourceCodes.Default;
}
