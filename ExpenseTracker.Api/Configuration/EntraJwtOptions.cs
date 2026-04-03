namespace ExpenseTracker.Api.Configuration;

/// <summary>
/// Optional Microsoft Entra (Azure AD / External ID) JWT validation. When <see cref="Enabled"/> is false,
/// only the local Identity-issued JWT (<see cref="JwtOptions"/>) is accepted.
/// </summary>
public sealed class EntraJwtOptions
{
    public const string SectionName = "Entra";

    /// <summary>When true, bearer tokens whose issuer looks like Entra/STS are validated with <see cref="Authority"/>.</summary>
    public bool Enabled { get; set; }

    /// <summary>Authority base, e.g. <c>https://login.microsoftonline.com/{tenantId}/v2.0</c> or External ID tenant.</summary>
    public string? Authority { get; set; }

    /// <summary>Application (API) audience: Application ID URI or client id, per app registration.</summary>
    public string? Audience { get; set; }
}
