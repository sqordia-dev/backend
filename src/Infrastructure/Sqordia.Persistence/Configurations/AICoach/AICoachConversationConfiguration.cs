using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities.AICoach;

namespace Sqordia.Persistence.Configurations.AICoach;

public class AICoachConversationConfiguration : IEntityTypeConfiguration<AICoachConversation>
{
    public void Configure(EntityTypeBuilder<AICoachConversation> builder)
    {
        builder.ToTable("AICoachConversations");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.UserId)
            .IsRequired();

        builder.Property(c => c.BusinessPlanId)
            .IsRequired();

        builder.Property(c => c.QuestionId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.QuestionNumber);

        builder.Property(c => c.QuestionText)
            .HasMaxLength(2000);

        builder.Property(c => c.Language)
            .IsRequired()
            .HasMaxLength(10)
            .HasDefaultValue("en");

        builder.Property(c => c.Persona)
            .HasMaxLength(50);

        builder.Property(c => c.TotalTokensUsed)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(c => c.LastMessageAt);

        builder.Property(c => c.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Navigation to messages
        builder.HasMany(c => c.Messages)
            .WithOne(m => m.Conversation)
            .HasForeignKey(m => m.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique index: one conversation per user/businessPlan/question
        builder.HasIndex(c => new { c.UserId, c.BusinessPlanId, c.QuestionId })
            .IsUnique()
            .HasDatabaseName("IX_AICoachConversations_UserId_BusinessPlanId_QuestionId");

        // Performance indexes
        builder.HasIndex(c => c.UserId)
            .HasDatabaseName("IX_AICoachConversations_UserId");

        builder.HasIndex(c => c.BusinessPlanId)
            .HasDatabaseName("IX_AICoachConversations_BusinessPlanId");

        builder.HasIndex(c => c.LastMessageAt)
            .HasDatabaseName("IX_AICoachConversations_LastMessageAt");
    }
}
