using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities;

namespace Sqordia.Persistence.Configurations;

public class ImpactMeasurementConfiguration : IEntityTypeConfiguration<ImpactMeasurement>
{
    public void Configure(EntityTypeBuilder<ImpactMeasurement> builder)
    {
        builder.ToTable("ImpactMeasurements");
        
        builder.HasKey(im => im.Id);
        
        builder.Property(im => im.MetricName)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(im => im.Description)
            .HasMaxLength(1000);
            
        builder.Property(im => im.MeasurementType)
            .HasMaxLength(100);
            
        builder.Property(im => im.UnitOfMeasurement)
            .HasMaxLength(50);
            
        builder.Property(im => im.BaselineValue)
            .HasPrecision(18, 2);
            
        builder.Property(im => im.TargetValue)
            .HasPrecision(18, 2);
            
        builder.Property(im => im.CurrentValue)
            .HasPrecision(18, 2);
            
        builder.Property(im => im.DataSource)
            .HasMaxLength(200);
            
        builder.Property(im => im.CollectionMethod)
            .HasMaxLength(200);
            
        builder.Property(im => im.Frequency)
            .HasMaxLength(50);
            
        builder.Property(im => im.ResponsibleParty)
            .HasMaxLength(200);
            
        builder.Property(im => im.Status)
            .HasMaxLength(50);
    }
}

