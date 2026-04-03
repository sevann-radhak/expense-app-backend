using System.Text.Json.Serialization;

namespace ExpenseTracker.IntegrationTests.Support;

internal sealed class AuthResponseDto
{
    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("accessToken")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("expiresAtUtc")]
    public DateTimeOffset ExpiresAtUtc { get; set; }

    [JsonPropertyName("roles")]
    public List<string> Roles { get; set; } = [];
}
