using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities;

namespace Sqordia.Persistence.Configurations;

public class AIPromptVersionConfiguration : IEntityTypeConfiguration<AIPromptVersion>
{
    public void Configure(EntityTypeBuilder<AIPromptVersion> builder)
    {
        builder.ToTable("AIPromptVersions");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Version)
            .IsRequired();

        builder.Property(v => v.SystemPrompt)
            .IsRequired()
            .HasColumnType("text");

        builder.Property(v => v.UserPromptTemplate)
            .IsRequired()
            .HasColumnType("text");

        builder.Property(v => v.Variables)
            .HasColumnType("text");

        builder.Property(v => v.Notes)
            .HasMaxLength(1000);

        builder.Property(v => v.ChangedBy)
            .HasMaxLength(256);

        builder.Property(v => v.ChangedAt)
            .IsRequired();

        // Relationships
        builder.HasOne(v => v.AIPrompt)
            .WithMany()
            .HasForeignKey(v => v.AIPromptId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(v => v.AIPromptId);
        builder.HasIndex(v => new { v.AIPromptId, v.Version })
            .IsUnique();
        builder.HasIndex(v => v.ChangedAt);
    }
}
