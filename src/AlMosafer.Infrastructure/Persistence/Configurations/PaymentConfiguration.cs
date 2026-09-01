using AlMosafer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlMosafer.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("payments");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.BookingId)
            .HasColumnName("booking_id");

        builder.Property(p => p.Amount)
            .HasPrecision(10, 2)
            .HasColumnName("amount");

        builder.Property(p => p.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasColumnName("status");

        builder.Property(p => p.TransactionId)
            .HasMaxLength(100)
            .HasColumnName("payment_id");

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at");

        // 1:1 Relationship with Booking
        builder.HasOne(p => p.Booking)
            .WithOne(b => b.Payment)
            .HasForeignKey<Payment>(p => p.BookingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
