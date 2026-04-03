using Microsoft.AspNetCore.Identity;

namespace ExpenseTracker.Infrastructure.Identity;

public sealed class ApplicationUser : IdentityUser
{
    public string? DisplayName { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
