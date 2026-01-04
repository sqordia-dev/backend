using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities;

namespace Sqordia.Persistence.Configurations;

public class TemplateFieldConfiguration : IEntityTypeConfiguration<TemplateField>
{
    public void Configure(EntityTypeBuilder<TemplateField> builder)
    {
        builder.ToTable("TemplateFields");
        
        builder.HasKey(tf => tf.Id);
        
        builder.Property(tf => tf.Name)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(tf => tf.Label)
            .HasMaxLength(200);
            
        builder.Property(tf => tf.FieldType)
            .HasMaxLength(50);
            
        builder.Property(tf => tf.MinValue)
            .HasPrecision(18, 2);
            
        builder.Property(tf => tf.MaxValue)
            .HasPrecision(18, 2);
            
        builder.Property(tf => tf.Format)
            .HasMaxLength(50);
            
        builder.Property(tf => tf.Pattern)
            .HasMaxLength(200);
    }
}

