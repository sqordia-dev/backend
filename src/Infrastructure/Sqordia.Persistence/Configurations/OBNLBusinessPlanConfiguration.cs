using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities;

namespace Sqordia.Persistence.Configurations;

public class OBNLBusinessPlanConfiguration : IEntityTypeConfiguration<OBNLBusinessPlan>
{
    public void Configure(EntityTypeBuilder<OBNLBusinessPlan> builder)
    {
        builder.ToTable("OBNLBusinessPlans");
        
        builder.HasKey(ob => ob.Id);
        
        builder.Property(ob => ob.OBNLType)
            .HasMaxLength(50);
            
        builder.Property(ob => ob.FundingRequirements)
            .HasPrecision(18, 2);
            
        builder.Property(ob => ob.LegalStructure)
            .HasMaxLength(100);
            
        builder.Property(ob => ob.RegistrationNumber)
            .HasMaxLength(100);
            
        builder.Property(ob => ob.GoverningBody)
            .HasMaxLength(200);
    }
}

