using AlMosafer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlMosafer.Infrastructure.Persistence.Configurations;

public class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.ToTable("conversations");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.BookingId)
            .HasColumnName("booking_id");

        builder.Property(c => c.TripId)
            .HasColumnName("trip_id");

        builder.Property(c => c.DriverId)
            .HasColumnName("driver_id");

        builder.Property(c => c.TravelerId)
            .HasColumnName("traveler_id");

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(c => c.LastMessageAt)
            .HasColumnName("last_message_at");

        // Relationships
        builder.HasOne(c => c.Booking)
            .WithOne(b => b.Conversation)
            .HasForeignKey<Conversation>(c => c.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Trip)
            .WithMany(t => t.Conversations)
            .HasForeignKey(c => c.TripId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Driver)
            .WithMany(u => u.DriverConversations)
            .HasForeignKey(c => c.DriverId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Traveler)
            .WithMany(u => u.TravelerConversations)
            .HasForeignKey(c => c.TravelerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
