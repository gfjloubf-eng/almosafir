namespace AlMosafer.Application.DTOs.Dashboard;

public class AdminSystemHealthDto
{
    public bool IsDatabaseConnected { get; set; }
    public string DatabaseProvider { get; set; } = "MySQL / MariaDB (XAMPP)";
    public string ApplicationStatus { get; set; } = "Healthy / Operational";
    public string EnvironmentName { get; set; } = "Development";
    public string RuntimeVersion { get; set; } = ".NET 10.0";
    public string SupportManagerName { get; set; } = "عمار عادل المصوعي";
    public string SupportManagerPhone { get; set; } = "712275038";
    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
}
