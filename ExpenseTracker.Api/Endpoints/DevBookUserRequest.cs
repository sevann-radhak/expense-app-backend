using System.Text.Json.Serialization;

namespace ExpenseTracker.Api.Endpoints;

internal sealed class DevBookUserRequest
{
    [JsonPropertyName("userId")]
    public string? UserId { get; set; }
}
