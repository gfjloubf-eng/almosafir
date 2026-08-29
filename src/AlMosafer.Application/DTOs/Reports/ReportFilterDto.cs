using AlMosafer.Domain.Enums;

namespace AlMosafer.Application.DTOs.Reports;

public class ReportFilterDto
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public UserRole? RoleFilter { get; set; }
}
