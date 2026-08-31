using AlMosafer.Domain.Enums;

namespace AlMosafer.Application.DTOs.Auth;

public class UserProfileDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public UserRole Role { get; set; }
    public string? Photo { get; set; }
    public string? PlateNumber { get; set; }
    public float Rating { get; set; }
    public string? VehicleModel { get; set; }
    public int? VehicleYear { get; set; }
    public string? City { get; set; }
    public int TotalTrips { get; set; }
    public decimal TotalEarnings { get; set; }
    public DateTime CreatedAt { get; set; }
}
