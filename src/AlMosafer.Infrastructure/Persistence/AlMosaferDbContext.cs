using AlMosafer.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AlMosafer.Infrastructure.Persistence;

public class AlMosaferDbContext : DbContext
{
    public AlMosaferDbContext(DbContextOptions<AlMosaferDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Trip> Trips => Set<Trip>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Rating> Ratings => Set<Rating>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<RouteLine> RouteLines => Set<RouteLine>();
    public DbSet<LineStop> LineStops => Set<LineStop>();
    public DbSet<LineSchedule> LineSchedules => Set<LineSchedule>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all EntityTypeConfigurations automatically from current assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AlMosaferDbContext).Assembly);
    }
}
