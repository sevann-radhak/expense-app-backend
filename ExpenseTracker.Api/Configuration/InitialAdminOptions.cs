namespace ExpenseTracker.Api.Configuration;

/// <summary>Optional startup seed: creates the first SuperAdmin when enabled (local/dev convenience). Disable in production; prefer bootstrap endpoint + secrets.</summary>
public sealed class InitialAdminOptions
{
    public const string SectionName = "InitialAdmin";

    public bool Enabled { get; set; }

    public string Email { get; set; } = "";

    public string Password { get; set; } = "";
}
