using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities;
using Sqordia.Domain.Enums;

namespace Sqordia.Persistence.Configurations;

public class FinancialCellConfiguration : IEntityTypeConfiguration<FinancialCell>
{
    public void Configure(EntityTypeBuilder<FinancialCell> builder)
    {
        builder.ToTable("FinancialCells");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.SheetName)
            .IsRequired()
            .HasMaxLength(100)
            .HasDefaultValue("Main");

        builder.Property(c => c.RowId)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.ColumnId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Value)
            .HasPrecision(18, 4);

        builder.Property(c => c.Formula)
            .HasMaxLength(1000);

        builder.Property(c => c.CellType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasDefaultValue(CellType.Number);

        builder.Property(c => c.DisplayFormat)
            .HasMaxLength(50);

        builder.Property(c => c.IsCalculated)
            .HasDefaultValue(false);

        builder.Property(c => c.IsLocked)
            .HasDefaultValue(false);

        // Composite index for efficient cell lookups
        builder.HasIndex(c => new { c.BusinessPlanId, c.SheetName, c.RowId, c.ColumnId })
            .IsUnique()
            .HasDatabaseName("IX_FinancialCells_BusinessPlan_Sheet_Row_Column");

        // Index for business plan queries
        builder.HasIndex(c => c.BusinessPlanId)
            .HasDatabaseName("IX_FinancialCells_BusinessPlanId");

        builder.HasIndex(c => c.IsCalculated)
            .HasDatabaseName("IX_FinancialCells_IsCalculated");

        // Relationship with BusinessPlan
        builder.HasOne(c => c.BusinessPlan)
            .WithMany()
            .HasForeignKey(c => c.BusinessPlanId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
