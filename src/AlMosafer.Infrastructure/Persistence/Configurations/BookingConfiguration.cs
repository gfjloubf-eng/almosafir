using AlMosafer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlMosafer.Infrastructure.Persistence.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("bookings");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.TripId)
            .HasColumnName("trip_id");

        builder.Property(b => b.TravelerId)
            .HasColumnName("traveler_id");

        builder.Property(b => b.SeatsBooked)
            .HasColumnName("seats_booked");

        builder.Property(b => b.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasColumnName("status");

        builder.Property(b => b.BookingTime)
            .HasColumnName("booking_time");

        // Relationships
        builder.HasOne(b => b.Trip)
            .WithMany(t => t.Bookings)
            .HasForeignKey(b => b.TripId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(b => b.Traveler)
            .WithMany(u => u.Bookings)
            .HasForeignKey(b => b.TravelerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
