using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using NetChallenge.Domain.Entities;
using NetChallenge.Infrastructure.External;
using NetChallenge.Infrastructure.Persistence;
using NetChallenge.Infrastructure.Security;

namespace NetChallenge.API.Tests.Infrastructure;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public FakeJsonPlaceholderHandler JsonPlaceholderHandler { get; }
    private SqliteConnection? _sqliteConnection;
    private ServiceProvider? _sqliteEfServices;

    public CustomWebApplicationFactory()
    {
        JsonPlaceholderHandler = new FakeJsonPlaceholderHandler(req =>
        {
            var path = req.RequestUri?.AbsolutePath ?? string.Empty;

            if (path.EndsWith("/users", StringComparison.OrdinalIgnoreCase))
            {
                // Minimal JSONPlaceholder users payload
                return FakeJsonPlaceholderHandler.Json(
                    System.Net.HttpStatusCode.OK,
                    """
                    [
                      { "id": 1, "name": "Leanne Graham", "username": "Bret", "email": "leanne@example.com", "phone": "1-770-736-8031", "website": "hildegard.org" },
                      { "id": 2, "name": "Ervin Howell", "username": "Antonette", "email": "ervin@example.com", "phone": "010-692-6593", "website": "anastasia.net" }
                    ]
                    """
                );
            }

            if (path.Contains("/users/", StringComparison.OrdinalIgnoreCase))
            {
                return FakeJsonPlaceholderHandler.Json(
                    System.Net.HttpStatusCode.OK,
                    """
                    { "id": 1, "name": "Leanne Graham", "username": "Bret", "email": "leanne@example.com", "phone": "1-770-736-8031", "website": "hildegard.org" }
                    """
                );
            }

            return FakeJsonPlaceholderHandler.Json(System.Net.HttpStatusCode.NotFound, "{}");
        });
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Replace AppDbContext with an in-memory SQLite DB.
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<AppDbContext>();
            services.RemoveAll<IConfigureOptions<DbContextOptions<AppDbContext>>>();
            services.RemoveAll<IPostConfigureOptions<DbContextOptions<AppDbContext>>>();
            services.RemoveAll(
                typeof(Microsoft.EntityFrameworkCore.Infrastructure.IDbContextOptionsConfiguration<AppDbContext>)
            );

            // Program.cs registers Npgsql provider services; remove them so we only have a single EF provider.
            services.RemoveAll<IDatabaseProvider>();

            // Keep a single open connection for the lifetime of the factory;
            // otherwise each DbContext gets its own empty in-memory DB.
            _sqliteConnection = new SqliteConnection("DataSource=:memory:");
            _sqliteConnection.Open();

            services.AddSingleton(_sqliteConnection);
            services.AddDbContext<AppDbContext>(
                (sp, options) =>
                {
                    var connection = sp.GetRequiredService<SqliteConnection>();
                    options.UseSqlite(connection);
                    _sqliteEfServices ??= new ServiceCollection()
                        .AddEntityFrameworkSqlite()
                        .BuildServiceProvider();
                    options.UseInternalServiceProvider(_sqliteEfServices);
                }
            );

            // Ensure the app uses our deterministic fake JsonPlaceholder client (no network).
            services.RemoveAll<JsonPlaceholderClient>();
            services.AddSingleton(sp =>
            {
                var httpClient = new HttpClient(JsonPlaceholderHandler)
                {
                    BaseAddress = new Uri("http://fake-jsonplaceholder.local/"),
                };
                return new JsonPlaceholderClient(httpClient);
            });

            // Build provider so we can open the SQLite connection + create schema + seed users.
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            db.Database.EnsureCreated();

            SeedTestUser(db);
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _sqliteConnection?.Dispose();
            _sqliteConnection = null;

            _sqliteEfServices?.Dispose();
            _sqliteEfServices = null;
        }
    }

    private static void SeedTestUser(AppDbContext db)
    {
        if (db.UserAccounts.Any(u => u.Username == "admin"))
        {
            return;
        }

        var (hash, salt) = PasswordHasher.HashPassword("admin123");

        db.UserAccounts.Add(
            new UserAccount
            {
                Id = Guid.NewGuid(),
                Username = "admin",
                PasswordHash = hash,
                PasswordSalt = salt,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                LastLoginAt = null,
            }
        );

        db.SaveChanges();
    }
}
