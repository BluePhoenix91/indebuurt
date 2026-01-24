using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pipeline.Core.Entities.Content;

namespace Pipeline.Core.Data.Configurations.Content;

public class NeighborhoodProseConfiguration : IEntityTypeConfiguration<NeighborhoodProse>
{
    public void Configure(EntityTypeBuilder<NeighborhoodProse> builder)
    {
        builder.ToTable("neighborhood_prose", "content");

        builder.HasKey(e => e.NisCode);
        builder.Property(e => e.NisCode).HasMaxLength(7);

        builder.Property(e => e.Slug).HasMaxLength(200).IsRequired();
        builder.HasIndex(e => e.Slug).IsUnique();

        builder.Property(e => e.City).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Name).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Intro).IsRequired();
        builder.Property(e => e.Subtitle).HasMaxLength(400).IsRequired();
        builder.Property(e => e.QualityScore).HasPrecision(4, 1);
        builder.Property(e => e.SeoQualityScore).HasPrecision(4, 1);
        builder.Property(e => e.PromptVersion).HasMaxLength(20);
        builder.Property(e => e.ModifiedBy).HasMaxLength(100);

        // FK to gis.neighborhoods deferred to Epic O when GIS entities are added
    }
}
