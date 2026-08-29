using AlMosafer.Domain.Enums;

namespace AlMosafer.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public UserRole Role { get; set; } = UserRole.Traveler;
    public string? Photo { get; set; }
    public string? PlateNumber { get; set; }
    public float Rating { get; set; } = 0.0f;
    public string? VehicleModel { get; set; }
    public int? VehicleYear { get; set; }
    public string? PreferencesJson { get; set; }
    public string? City { get; set; }
    public int TotalTrips { get; set; } = 0;
    public decimal TotalEarnings { get; set; } = 0.00m;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public ICollection<Trip> DrivenTrips { get; set; } = new List<Trip>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<Message> SentMessages { get; set; } = new List<Message>();
    public ICollection<Rating> GivenRatings { get; set; } = new List<Rating>();
    public ICollection<Rating> ReceivedRatings { get; set; } = new List<Rating>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<Conversation> DriverConversations { get; set; } = new List<Conversation>();
    public ICollection<Conversation> TravelerConversations { get; set; } = new List<Conversation>();
}
