using Microsoft.EntityFrameworkCore;

namespace Pipeline.Core.Data;

public class PipelineDbContext : DbContext
{
    public PipelineDbContext(DbContextOptions<PipelineDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure schemas - entities will be added in Epic O
        // modelBuilder.HasDefaultSchema("gis");

        base.OnModelCreating(modelBuilder);
    }
}
