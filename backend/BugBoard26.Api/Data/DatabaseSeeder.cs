using BugBoard26.Api.Models;
using BugBoard26.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace BugBoard26.Api.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAdminAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await dbContext.Database.MigrateAsync();

        if (await dbContext.Users.AnyAsync(user => user.Role == UserRole.Admin))
        {
            return;
        }

        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var email = NormalizeEmail(configuration["DefaultAdmin:Email"] ?? "admin@bugboard26.local");
        var password = configuration["DefaultAdmin:Password"] ?? "Admin123!";

        dbContext.Users.Add(new User
        {
            Email = email,
            PasswordHash = passwordHasher.Hash(password),
            Role = UserRole.Admin,
            IsActive = true
        });

        await dbContext.SaveChangesAsync();
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }
}
