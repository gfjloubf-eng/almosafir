using AlMosafer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlMosafer.Infrastructure.Persistence.Configurations;

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("messages");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.ConversationId)
            .HasColumnName("conversation_id");

        builder.Property(m => m.SenderId)
            .HasColumnName("sender_id");

        builder.Property(m => m.Text)
            .IsRequired()
            .HasColumnName("message");

        builder.Property(m => m.IsRead)
            .HasColumnName("is_read");

        builder.Property(m => m.CreatedAt)
            .HasColumnName("created_at");

        // Relationships
        builder.HasOne(m => m.Conversation)
            .WithMany(c => c.Messages)
            .HasForeignKey(m => m.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.Sender)
            .WithMany(u => u.SentMessages)
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
