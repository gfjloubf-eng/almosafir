using AlMosafer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlMosafer.Infrastructure.Persistence.Configurations;

public class RatingConfiguration : IEntityTypeConfiguration<Rating>
{
    public void Configure(EntityTypeBuilder<Rating> builder)
    {
        builder.ToTable("ratings");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.TripId)
            .HasColumnName("trip_id");

        builder.Property(r => r.TravelerId)
            .HasColumnName("traveler_id");

        builder.Property(r => r.DriverId)
            .HasColumnName("driver_id");

        builder.Property(r => r.Value)
            .HasColumnName("rating");

        builder.Property(r => r.Comment)
            .HasColumnName("comment");

        builder.Property(r => r.CreatedAt)
            .HasColumnName("created_at");

        // Relationships
        builder.HasOne(r => r.Trip)
            .WithMany(t => t.Ratings)
            .HasForeignKey(r => r.TripId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Traveler)
            .WithMany(u => u.GivenRatings)
            .HasForeignKey(r => r.TravelerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Driver)
            .WithMany(u => u.ReceivedRatings)
            .HasForeignKey(r => r.DriverId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
