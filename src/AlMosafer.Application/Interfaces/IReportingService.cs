using AlMosafer.Application.DTOs.Reports;

namespace AlMosafer.Application.Interfaces;

public interface IReportingService
{
    Task<AdminReportSummaryDto> GetAdminReportSummaryAsync(ReportFilterDto filter);
    Task<IEnumerable<DriverPerformanceDto>> GetTopDriversAsync(int count = 4);
}
