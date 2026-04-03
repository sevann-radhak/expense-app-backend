namespace ExpenseTracker.Api.Configuration;

public sealed class SetupOptions
{
    public const string SectionName = "Setup";

    public string? BootstrapToken { get; set; }
}
