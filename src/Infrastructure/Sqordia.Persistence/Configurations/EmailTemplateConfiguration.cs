using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities;

namespace Sqordia.Persistence.Configurations;

public class EmailTemplateConfiguration : IEntityTypeConfiguration<EmailTemplate>
{
    public void Configure(EntityTypeBuilder<EmailTemplate> builder)
    {
        builder.ToTable("email_templates");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.Category)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.SubjectFr)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(e => e.SubjectEn)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(e => e.BodyFr)
            .IsRequired();

        builder.Property(e => e.BodyEn)
            .IsRequired();

        builder.Property(e => e.VariablesJson)
            .HasColumnType("jsonb");

        builder.HasIndex(e => new { e.Name, e.IsActive });
        builder.HasIndex(e => e.Category);

        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
