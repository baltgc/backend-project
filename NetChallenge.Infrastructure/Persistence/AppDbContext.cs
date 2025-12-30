using Microsoft.EntityFrameworkCore;
using NetChallenge.Domain.Entities;

namespace NetChallenge.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<UserAccount> UserAccounts => Set<UserAccount>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();

    public DbSet<CacheEntry> CacheEntries => Set<CacheEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserAccount>(b =>
        {
            b.ToTable("user_accounts");
            b.HasKey(x => x.Id);
            b.Property(x => x.Username).IsRequired().HasMaxLength(128);
            b.HasIndex(x => x.Username).IsUnique();
            b.Property(x => x.PasswordHash).IsRequired().HasMaxLength(512);
            b.Property(x => x.PasswordSalt).IsRequired().HasMaxLength(512);
            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.IsActive).IsRequired();
        });

        modelBuilder.Entity<RefreshToken>(b =>
        {
            b.ToTable("refresh_tokens");
            b.HasKey(x => x.Id);
            b.Property(x => x.TokenHash).IsRequired().HasMaxLength(128);
            b.HasIndex(x => x.TokenHash).IsUnique();

            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.ExpiresAt).IsRequired();
            b.Property(x => x.CreatedByIp).HasMaxLength(64);
            b.Property(x => x.CreatedByUserAgent).HasMaxLength(256);

            b.HasOne(x => x.UserAccount)
                .WithMany()
                .HasForeignKey(x => x.UserAccountId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.ReplacedByToken)
                .WithOne()
                .HasForeignKey<RefreshToken>(x => x.ReplacedByTokenId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<AuditEvent>(b =>
        {
            b.ToTable("audit_events");
            b.HasKey(x => x.Id);
            b.Property(x => x.Type).IsRequired().HasMaxLength(64);
            b.Property(x => x.Timestamp).IsRequired();
            b.Property(x => x.CorrelationId).IsRequired().HasMaxLength(128);
            b.Property(x => x.Path).HasMaxLength(512);
            b.Property(x => x.Method).HasMaxLength(16);
            b.Property(x => x.MetadataJson).HasMaxLength(4000);

            b.HasOne(x => x.UserAccount)
                .WithMany()
                .HasForeignKey(x => x.UserAccountId)
                .OnDelete(DeleteBehavior.SetNull);

            b.HasIndex(x => x.Timestamp);
            b.HasIndex(x => x.Type);
        });

        modelBuilder.Entity<CacheEntry>(b =>
        {
            b.ToTable("cache_entries");
            b.HasKey(x => x.Id);
            b.Property(x => x.Key).IsRequired().HasMaxLength(256);
            b.HasIndex(x => x.Key).IsUnique();
            b.Property(x => x.PayloadJson).IsRequired();
            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.ExpiresAt).IsRequired();
            b.HasIndex(x => x.ExpiresAt);
        });

        base.OnModelCreating(modelBuilder);
    }
}
