namespace ExpenseTracker.Api.Configuration;

/// <summary>One-time superadmin creation when no SuperAdmin exists. Send as header X-Setup-Token.</summary>
public sealed class SetupOptions
{
    public const string SectionName = "Setup";

    public string? BootstrapToken { get; set; }
}
