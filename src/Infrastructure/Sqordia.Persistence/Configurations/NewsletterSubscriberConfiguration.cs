using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sqordia.Domain.Entities;

namespace Sqordia.Persistence.Configurations;

public class NewsletterSubscriberConfiguration : IEntityTypeConfiguration<NewsletterSubscriber>
{
    public void Configure(EntityTypeBuilder<NewsletterSubscriber> builder)
    {
        builder.ToTable("newsletter_subscribers");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.HasIndex(n => n.Email)
            .IsUnique()
            .HasDatabaseName("IX_newsletter_subscribers_email");

        builder.Property(n => n.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(n => n.Language)
            .IsRequired()
            .HasMaxLength(5)
            .HasDefaultValue("fr");

        builder.Property(n => n.SubscribedAt)
            .IsRequired();

        builder.HasIndex(n => n.IsActive)
            .HasDatabaseName("IX_newsletter_subscribers_is_active");
    }
}
