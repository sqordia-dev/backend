using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities.BusinessPlan;

namespace Sqordia.Persistence.Configurations.BusinessPlan;

public class QuestionnaireTemplateConfiguration : IEntityTypeConfiguration<QuestionnaireTemplate>
{
    public void Configure(EntityTypeBuilder<QuestionnaireTemplate> builder)
    {
        builder.ToTable("QuestionnaireTemplates");
        
        builder.HasKey(qt => qt.Id);
        
        builder.Property(qt => qt.Name)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(qt => qt.Description)
            .HasMaxLength(1000);
            
        builder.Property(qt => qt.PlanType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);
            
        builder.Property(qt => qt.IsActive)
            .IsRequired()
            .HasDefaultValue(true);
            
        builder.Property(qt => qt.Version)
            .IsRequired()
            .HasDefaultValue(1);
        
        // Relationships
        builder.HasMany(qt => qt.Questions)
            .WithOne(q => q.QuestionnaireTemplate)
            .HasForeignKey(q => q.QuestionnaireTemplateId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Indexes
        builder.HasIndex(qt => qt.PlanType);
        builder.HasIndex(qt => qt.IsActive);
        builder.HasIndex(qt => new { qt.PlanType, qt.IsActive, qt.Version });
    }
}

