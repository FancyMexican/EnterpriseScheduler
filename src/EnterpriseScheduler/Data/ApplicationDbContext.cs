using Microsoft.EntityFrameworkCore;
using EnterpriseScheduler.Models;

namespace EnterpriseScheduler.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Meeting> Meetings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.TimeZone).IsRequired();
        });

        modelBuilder.Entity<Meeting>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired();
            entity.Property(e => e.StartTime).IsRequired();
            entity.Property(e => e.EndTime).IsRequired();

            // Add indexes for efficient time range queries
            entity.HasIndex(e => e.StartTime);
            entity.HasIndex(e => e.EndTime);
        });

        modelBuilder.Entity<Meeting>()
            .HasMany(m => m.Participants)
            .WithMany(u => u.Meetings);
    }
}
