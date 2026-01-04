using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities.BusinessPlan;

namespace Sqordia.Persistence.Configurations.BusinessPlan;

public class QuestionTemplateConfiguration : IEntityTypeConfiguration<QuestionTemplate>
{
    public void Configure(EntityTypeBuilder<QuestionTemplate> builder)
    {
        builder.ToTable("QuestionTemplates");
        
        builder.HasKey(q => q.Id);
        
        builder.Property(q => q.QuestionText)
            .IsRequired()
            .HasMaxLength(1000);
            
        builder.Property(q => q.HelpText)
            .HasMaxLength(500);
            
        builder.Property(q => q.QuestionType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);
            
        builder.Property(q => q.Section)
            .HasMaxLength(100);
            
        builder.Property(q => q.Options)
            .HasColumnType("text"); // JSON array
            
        builder.Property(q => q.ValidationRules)
            .HasColumnType("text"); // JSON
            
        builder.Property(q => q.ConditionalLogic)
            .HasColumnType("text"); // JSON
        
        // Relationships
        builder.HasOne(q => q.QuestionnaireTemplate)
            .WithMany(qt => qt.Questions)
            .HasForeignKey(q => q.QuestionnaireTemplateId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasMany(q => q.Responses)
            .WithOne(r => r.QuestionTemplate)
            .HasForeignKey(r => r.QuestionTemplateId)
            .OnDelete(DeleteBehavior.Restrict); // Don't cascade delete responses
        
        // Indexes
        builder.HasIndex(q => q.QuestionnaireTemplateId);
        builder.HasIndex(q => new { q.QuestionnaireTemplateId, q.Order });
    }
}

