using System.Text.Json.Serialization;

namespace ExpenseTracker.IntegrationTests.Support;

internal sealed class UserProfileDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

    [JsonPropertyName("roles")]
    public List<string> Roles { get; set; } = [];
}
