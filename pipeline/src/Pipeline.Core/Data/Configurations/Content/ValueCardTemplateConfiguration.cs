using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pipeline.Core.Data.Seeds;
using Pipeline.Core.Entities.Content;

namespace Pipeline.Core.Data.Configurations.Content;

public class ValueCardTemplateConfiguration : IEntityTypeConfiguration<ValueCardTemplate>
{
    public void Configure(EntityTypeBuilder<ValueCardTemplate> builder)
    {
        builder.ToTable("value_card_templates", "content");

        builder.HasKey(e => e.CardType);
        builder.Property(e => e.CardType)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(e => e.Title).HasMaxLength(100).IsRequired();

        // Seed data from ValueCardTemplateSeeder
        builder.HasData(ValueCardTemplateSeeder.GetTemplates());
    }
}
