using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pipeline.Core.Entities.Content;

namespace Pipeline.Core.Data.Configurations.Content;

public class LabelRuleConfiguration : IEntityTypeConfiguration<LabelRule>
{
    public void Configure(EntityTypeBuilder<LabelRule> builder)
    {
        builder.ToTable("label_rules", "content");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.Category)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.LabelText).HasMaxLength(100).IsRequired();
        builder.Property(e => e.LabelIcon).HasMaxLength(100).IsRequired();

        builder.Property(e => e.ConditionField)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.ConditionOperator)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(e => e.ConditionValue).HasMaxLength(50).IsRequired();
    }
}
