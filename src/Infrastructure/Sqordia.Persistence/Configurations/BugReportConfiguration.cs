using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities;

namespace Sqordia.Persistence.Configurations;

public class BugReportConfiguration : IEntityTypeConfiguration<BugReport>
{
    public void Configure(EntityTypeBuilder<BugReport> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(b => b.PageSection)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(b => b.Severity)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(b => b.Description)
            .IsRequired();

        builder.Property(b => b.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(b => b.TicketNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(b => b.AppVersion)
            .HasMaxLength(50);

        builder.Property(b => b.Browser)
            .HasMaxLength(100);

        builder.Property(b => b.OperatingSystem)
            .HasMaxLength(100);

        builder.Property(b => b.ResolutionNotes)
            .HasMaxLength(2000);

        builder.HasMany(b => b.Attachments)
            .WithOne(a => a.BugReport)
            .HasForeignKey(a => a.BugReportId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(b => b.TicketNumber)
            .IsUnique();
        builder.HasIndex(b => b.Status);
        builder.HasIndex(b => b.Severity);
        builder.HasIndex(b => b.ReportedByUserId);
        builder.HasIndex(b => b.Created);
    }
}
