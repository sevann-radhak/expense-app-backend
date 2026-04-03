using ExpenseTracker.Api.Configuration;
using ExpenseTracker.Infrastructure.Identity;
using ExpenseTracker.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace ExpenseTracker.Api.Hosting;

/// <summary>Ensures roles exist; optionally creates the first SuperAdmin from configuration.</summary>
public sealed class IdentitySeedHostedService(
    IServiceProvider services,
    ILogger<IdentitySeedHostedService> logger,
    IOptions<InitialAdminOptions> initialAdminOptions)
    : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using IServiceScope scope = services.CreateScope();
        RoleManager<IdentityRole> roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        UserManager<ApplicationUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        await IdentityDataSeeder.EnsureRolesExistAsync(roleManager, cancellationToken).ConfigureAwait(false);

        InitialAdminOptions opts = initialAdminOptions.Value;
        if (!opts.Enabled || string.IsNullOrWhiteSpace(opts.Email) || string.IsNullOrWhiteSpace(opts.Password))
        {
            return;
        }

        IList<ApplicationUser> existingSuper = await userManager.GetUsersInRoleAsync(AppRoles.SuperAdmin)
            .ConfigureAwait(false);
        if (existingSuper.Count > 0)
        {
            return;
        }

        ApplicationUser? existing = await userManager.FindByEmailAsync(opts.Email).ConfigureAwait(false);
        if (existing is not null)
        {
            logger.LogWarning("InitialAdmin email exists but is not SuperAdmin; skipping automatic SuperAdmin seed.");
            return;
        }

        var user = new ApplicationUser
        {
            UserName = opts.Email.Trim(),
            Email = opts.Email.Trim(),
            EmailConfirmed = true,
            CreatedAtUtc = DateTime.UtcNow,
        };
        IdentityResult create = await userManager.CreateAsync(user, opts.Password).ConfigureAwait(false);
        if (!create.Succeeded)
        {
            logger.LogError(
                "InitialAdmin seed failed: {Errors}",
                string.Join("; ", create.Errors.Select(e => e.Description)));
            return;
        }

        IdentityResult roleAdd = await userManager.AddToRoleAsync(user, AppRoles.SuperAdmin).ConfigureAwait(false);
        if (!roleAdd.Succeeded)
        {
            logger.LogError(
                "InitialAdmin role assignment failed: {Errors}",
                string.Join("; ", roleAdd.Errors.Select(e => e.Description)));
        }
        else
        {
            logger.LogInformation("Seeded SuperAdmin user for email {Email} (InitialAdmin:Enabled).", opts.Email);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
