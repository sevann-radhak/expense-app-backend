namespace ExpenseTracker.Infrastructure.Identity;

public static class AppRoles
{
    public const string SuperAdmin = "SuperAdmin";

    public const string Admin = "Admin";

    public const string User = "User";

    public static IReadOnlyList<string> All { get; } = [SuperAdmin, Admin, User];
}
