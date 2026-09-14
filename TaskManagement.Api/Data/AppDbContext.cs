using Microsoft.EntityFrameworkCore;
using TaskManagement.Api.Models;

namespace TaskManagement.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TaskItem>().ToTable("tasks");
        modelBuilder.Entity<User>().ToTable("users");
        modelBuilder.Entity<UserProfile>().ToTable("user_profiles");
        modelBuilder.Entity<RefreshToken>().ToTable("refresh_tokens");

        modelBuilder.Entity<User>()
            .Property(user => user.Role)
            .HasDefaultValue("User");

        modelBuilder.Entity<User>()
            .HasIndex(user => user.Email)
            .IsUnique();

        modelBuilder.Entity<TaskItem>()
            .HasIndex(task => task.UserId);

        modelBuilder.Entity<TaskItem>()
            .HasIndex(task => new { task.UserId, task.IsDeleted, task.Id });

        modelBuilder.Entity<TaskItem>()
            .Property(task => task.CreatedAt)
            .HasDefaultValueSql("now()");

        modelBuilder.Entity<TaskItem>()
            .HasOne(t => t.User)
            .WithMany(u => u.Tasks)
            .HasForeignKey(t => t.UserId);

        modelBuilder.Entity<RefreshToken>()
            .HasIndex(refreshToken => refreshToken.TokenHash)
            .IsUnique();

        modelBuilder.Entity<RefreshToken>()
            .HasIndex(refreshToken => refreshToken.UserId);

        modelBuilder.Entity<RefreshToken>()
            .HasOne(refreshToken => refreshToken.User)
            .WithMany(user => user.RefreshTokens)
            .HasForeignKey(refreshToken => refreshToken.UserId);

        modelBuilder.Entity<UserProfile>()
            .HasIndex(profile => profile.UserId)
            .IsUnique();

        modelBuilder.Entity<UserProfile>()
            .Property(profile => profile.CreatedAt)
            .HasDefaultValueSql("now()");

        modelBuilder.Entity<UserProfile>()
            .HasOne(profile => profile.User)
            .WithOne(user => user.Profile)
            .HasForeignKey<UserProfile>(profile => profile.UserId);
    }
}
