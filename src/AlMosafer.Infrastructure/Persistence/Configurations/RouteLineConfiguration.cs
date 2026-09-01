using AlMosafer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlMosafer.Infrastructure.Persistence.Configurations;

public class RouteLineConfiguration : IEntityTypeConfiguration<RouteLine>
{
    public void Configure(EntityTypeBuilder<RouteLine> builder)
    {
        builder.ToTable("route_lines");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Name)
            .IsRequired()
            .HasMaxLength(150)
            .HasColumnName("name");

        builder.Property(l => l.City)
            .IsRequired()
            .HasMaxLength(100)
            .HasColumnName("city");

        builder.Property(l => l.Description)
            .HasColumnName("description");

        builder.Property(l => l.IsActive)
            .HasColumnName("is_active");

        builder.Property(l => l.CreatedAt)
            .HasColumnName("created_at");
    }
}

public class LineStopConfiguration : IEntityTypeConfiguration<LineStop>
{
    public void Configure(EntityTypeBuilder<LineStop> builder)
    {
        builder.ToTable("line_stops");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.LineId)
            .HasColumnName("line_id");

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(150)
            .HasColumnName("name");

        builder.Property(s => s.OrderIndex)
            .HasColumnName("order_index");

        builder.HasOne(s => s.Line)
            .WithMany(l => l.Stops)
            .HasForeignKey(s => s.LineId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class LineScheduleConfiguration : IEntityTypeConfiguration<LineSchedule>
{
    public void Configure(EntityTypeBuilder<LineSchedule> builder)
    {
        builder.ToTable("line_schedules");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.LineId)
            .HasColumnName("line_id");

        builder.Property(s => s.DayOfWeek)
            .HasColumnName("day_of_week");

        builder.Property(s => s.DepartureTime)
            .HasColumnName("departure_time");

        builder.HasOne(s => s.Line)
            .WithMany(l => l.Schedules)
            .HasForeignKey(s => s.LineId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
