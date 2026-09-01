namespace AlMosafer.Application.DTOs.Ratings;

public class DriverRatingSummaryDto
{
    public int DriverId { get; set; }
    public string DriverName { get; set; } = string.Empty;
    public float AverageRating { get; set; }
    public int TotalRatingsCount { get; set; }
    public IEnumerable<RatingDetailsDto> RecentRatings { get; set; } = new List<RatingDetailsDto>();
}
