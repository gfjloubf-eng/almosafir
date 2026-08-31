using AlMosafer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlMosafer.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Name)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("name");

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("email");

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnName("password");

        builder.Property(u => u.Phone)
            .HasMaxLength(20)
            .HasColumnName("phone");

        builder.Property(u => u.Role)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasColumnName("role");

        builder.Property(u => u.Photo)
            .HasMaxLength(255)
            .HasColumnName("photo");

        builder.Property(u => u.PlateNumber)
            .HasMaxLength(20)
            .HasColumnName("plate_number");

        builder.Property(u => u.Rating)
            .HasColumnName("rating");

        builder.Property(u => u.VehicleModel)
            .HasMaxLength(50)
            .HasColumnName("vehicle_model");

        builder.Property(u => u.VehicleYear)
            .HasColumnName("vehicle_year");

        builder.Property(u => u.PreferencesJson)
            .HasColumnName("preferences");

        builder.Property(u => u.City)
            .HasMaxLength(100)
            .HasColumnName("city");

        builder.Property(u => u.TotalTrips)
            .HasColumnName("total_trips");

        builder.Property(u => u.TotalEarnings)
            .HasPrecision(10, 2)
            .HasColumnName("total_earnings");

        builder.Property(u => u.CreatedAt)
            .HasColumnName("created_at");
    }
}
