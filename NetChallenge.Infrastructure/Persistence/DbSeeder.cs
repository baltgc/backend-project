using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetChallenge.Domain.Entities;
using NetChallenge.Infrastructure.Security;

namespace NetChallenge.Infrastructure.Persistence;

public static class DbSeeder
{
    public static async Task SeedAdminAsync(
        AppDbContext db,
        ILogger logger,
        string username,
        string password
    )
    {
        username = username.Trim();

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning("Skipping admin seed because username/password is missing.");
            return;
        }

        var exists = await db.UserAccounts.AnyAsync(u => u.Username == username);
        if (exists)
        {
            return;
        }

        var (hash, salt) = PasswordHasher.HashPassword(password);

        db.UserAccounts.Add(
            new UserAccount
            {
                Id = Guid.NewGuid(),
                Username = username,
                PasswordHash = hash,
                PasswordSalt = salt,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
            }
        );

        await db.SaveChangesAsync();
        logger.LogInformation("Seeded admin user '{Username}'.", username);
    }
}
