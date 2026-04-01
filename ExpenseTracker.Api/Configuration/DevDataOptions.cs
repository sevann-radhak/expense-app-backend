namespace ExpenseTracker.Api.Configuration;

public sealed class DevDataOptions
{
    public const string SectionName = "DevData";

    /// <summary>When true, registers <c>/api/dev/*</c> book reset and seed routes.</summary>
    public bool ExposeEndpoints { get; set; }

    /// <summary>When true, requests must send header <c>X-Dev-Data-Secret</c> matching <see cref="SharedSecret"/>.</summary>
    public bool RequireSharedSecret { get; set; }

    public string? SharedSecret { get; set; }
}
