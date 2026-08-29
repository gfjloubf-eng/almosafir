using AlMosafer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlMosafer.Infrastructure.Persistence.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.UserId)
            .HasColumnName("user_id");

        builder.Property(n => n.Title)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnName("title");

        builder.Property(n => n.Message)
            .HasColumnName("message");

        builder.Property(n => n.Type)
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasColumnName("type");

        builder.Property(n => n.IsRead)
            .HasColumnName("read");

        builder.Property(n => n.CreatedAt)
            .HasColumnName("created_at");

        // Relationships
        builder.HasOne(n => n.User)
            .WithMany(u => u.Notifications)
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
