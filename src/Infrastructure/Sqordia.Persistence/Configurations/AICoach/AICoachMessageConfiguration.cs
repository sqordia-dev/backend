using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities.AICoach;

namespace Sqordia.Persistence.Configurations.AICoach;

public class AICoachMessageConfiguration : IEntityTypeConfiguration<AICoachMessage>
{
    public void Configure(EntityTypeBuilder<AICoachMessage> builder)
    {
        builder.ToTable("AICoachMessages");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.ConversationId)
            .IsRequired();

        builder.Property(m => m.Role)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(m => m.Content)
            .IsRequired();

        builder.Property(m => m.TokenCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(m => m.Sequence)
            .IsRequired();

        // Performance indexes
        builder.HasIndex(m => m.ConversationId)
            .HasDatabaseName("IX_AICoachMessages_ConversationId");

        builder.HasIndex(m => new { m.ConversationId, m.Sequence })
            .HasDatabaseName("IX_AICoachMessages_ConversationId_Sequence");

        builder.HasIndex(m => m.Created)
            .HasDatabaseName("IX_AICoachMessages_Created");
    }
}
