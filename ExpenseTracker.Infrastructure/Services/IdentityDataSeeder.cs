using ExpenseTracker.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace ExpenseTracker.Infrastructure.Services;

public static class IdentityDataSeeder
{
    public static async Task EnsureRolesExistAsync(RoleManager<IdentityRole> roleManager, CancellationToken cancellationToken = default)
    {
        foreach (string roleName in AppRoles.All)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!await roleManager.RoleExistsAsync(roleName).ConfigureAwait(false))
            {
                IdentityResult r = await roleManager.CreateAsync(new IdentityRole(roleName)).ConfigureAwait(false);
                if (!r.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Failed to create role {roleName}: {string.Join("; ", r.Errors.Select(e => e.Description))}");
                }
            }
        }
    }
}
