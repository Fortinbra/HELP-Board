using HelpBoard.Abstractions.Domain;
using Microsoft.EntityFrameworkCore;

namespace HelpBoard.Repositories.Data;

/// <summary>Entity Framework Core database context for the HELP-Board application.</summary>
public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Ticket> Tickets => Set<Ticket>();

    // Feature 004 placeholders: DbSet<ProjectBoard> and DbSet<WorkItem>.
    // Feature 003 placeholders: DbSet<StrategicObjective>, DbSet<StrategicInitiative>, and DbSet<StrategicMilestone>.

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Title).HasMaxLength(200).IsRequired();
            entity.Property(t => t.Description).HasMaxLength(4000).IsRequired();
            entity.Property(t => t.CreatedBy).HasMaxLength(100).IsRequired();
            entity.Property(t => t.Status).HasConversion<string>();
            entity.Property(t => t.Priority).HasConversion<string>();

            entity.HasIndex(t => t.CreatedAt);
            entity.HasIndex(t => t.Status);
        });

        // Feature 004 placeholder: configure ProjectBoard and WorkItem mappings here.
        // Feature 003 placeholder: configure Strategic hierarchy mappings and relationships here.
    }
}
