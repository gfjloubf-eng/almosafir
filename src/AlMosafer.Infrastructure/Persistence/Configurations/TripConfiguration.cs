using AlMosafer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlMosafer.Infrastructure.Persistence.Configurations;

public class TripConfiguration : IEntityTypeConfiguration<Trip>
{
    public void Configure(EntityTypeBuilder<Trip> builder)
    {
        builder.ToTable("trips");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.DriverId)
            .HasColumnName("driver_id");

        builder.Property(t => t.FromCity)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("from_city");

        builder.Property(t => t.FromLocation)
            .HasMaxLength(100)
            .HasColumnName("from_location");

        builder.Property(t => t.ToCity)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("to_city");

        builder.Property(t => t.TripTime)
            .HasColumnName("trip_time");

        builder.Property(t => t.Seats)
            .HasColumnName("seats");

        builder.Property(t => t.PricePerSeat)
            .HasPrecision(10, 2)
            .HasColumnName("price_per_seat");

        builder.Property(t => t.Description)
            .HasColumnName("description");

        builder.Property(t => t.VehicleInfo)
            .HasMaxLength(255)
            .HasColumnName("vehicle_info");

        builder.Property(t => t.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasColumnName("status");

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at");

        // Relationships
        builder.HasOne(t => t.Driver)
            .WithMany(u => u.DrivenTrips)
            .HasForeignKey(t => t.DriverId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
