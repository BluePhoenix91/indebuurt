using Microsoft.EntityFrameworkCore;
using Pipeline.Core.Entities.Content;

namespace Pipeline.Core.Data;

public class PipelineDbContext : DbContext
{
    public PipelineDbContext(DbContextOptions<PipelineDbContext> options)
        : base(options)
    {
    }

    // Content schema entities
    public DbSet<NeighborhoodProse> NeighborhoodProse => Set<NeighborhoodProse>();
    public DbSet<ValueCardTemplate> ValueCardTemplates => Set<ValueCardTemplate>();
    public DbSet<LabelRule> LabelRules => Set<LabelRule>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply all configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PipelineDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
