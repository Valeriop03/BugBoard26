using BugBoard26.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BugBoard26.Api.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Issue> Issues => Set<Issue>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(user => user.Email).IsUnique();
            entity.Property(user => user.Email).HasMaxLength(180).IsRequired();
            entity.Property(user => user.PasswordHash).HasMaxLength(500).IsRequired();
            entity.Property(user => user.Role).HasConversion<string>().HasMaxLength(30).IsRequired();
        });

        modelBuilder.Entity<Issue>(entity =>
        {
            entity.Property(issue => issue.Title).HasMaxLength(200).IsRequired();
            entity.Property(issue => issue.Description).HasMaxLength(4000).IsRequired();
            entity.Property(issue => issue.Type).HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.Property(issue => issue.Priority).HasConversion<string>().HasMaxLength(30);
            entity.Property(issue => issue.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
            entity.Property(issue => issue.ImagePath).HasMaxLength(500);

            entity.HasOne(issue => issue.CreatedBy)
                .WithMany(user => user.CreatedIssues)
                .HasForeignKey(issue => issue.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(issue => issue.AssignedTo)
                .WithMany(user => user.AssignedIssues)
                .HasForeignKey(issue => issue.AssignedToId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(issue => issue.DuplicateOfIssue)
                .WithMany()
                .HasForeignKey(issue => issue.DuplicateOfIssueId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.Property(notification => notification.Message).HasMaxLength(500).IsRequired();

            entity.HasOne(notification => notification.User)
                .WithMany(user => user.Notifications)
                .HasForeignKey(notification => notification.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(notification => notification.Issue)
                .WithMany(issue => issue.Notifications)
                .HasForeignKey(notification => notification.IssueId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
