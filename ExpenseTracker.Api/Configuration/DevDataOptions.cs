namespace ExpenseTracker.Api.Configuration;

public sealed class DevDataOptions
{
    public const string SectionName = "DevData";

    public bool ExposeEndpoints { get; set; }

    public bool RequireSharedSecret { get; set; }

    public string? SharedSecret { get; set; }
}
