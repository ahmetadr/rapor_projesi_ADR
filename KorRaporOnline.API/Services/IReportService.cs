// KorRaporOnline.API/Services/Interfaces/IReportService.cs
using KorRaporOnline.API.Models;
using KorRaporOnline.API.Models.Common;
using KorRaporOnline.API.Models.DTOs;

public interface IReportService
{
    Task<ServiceResponse<bool>> ValidateQueryAsync(string query);
    Task<ServiceResponse<IEnumerable<ReportDto>>> GetAllReportsAsync();
    Task<ServiceResponse<ReportDto>> GetReportByIdAsync(int id);
    Task<ServiceResponse<IEnumerable<ReportDto>>> SearchReportsAsync(string searchTerm, int page, int pageSize);
    Task<ServiceResponse<ReportDto>> CreateReportAsync(ReportCreateDto createReportDto);
    Task<ServiceResponse<ReportDto>> UpdateReportAsync(int id, ReportUpdateDto reportDto);
    Task<ServiceResponse<bool>> DeleteReportAsync(int id);
    Task<ServiceResponse<IEnumerable<ReportDto>>> GetUserReports(int userId); // Temel metod
    Task<ServiceResponse<PaginatedResult<ReportDto>>> GetUserReportsPaginated(int userId, int page, int pageSize); // Paginated versiyon
    Task<ServiceResponse<ReportDto>> GetReport(int userId, int reportId);
    Task<ServiceResponse<ReportDto>> SaveReport(int userId, SavedReportDto report, ReportDefinition definition);
    Task<ServiceResponse<bool>> DeleteReport(int userId, int reportId);
    Task<ServiceResponse<object>> ExecuteReport(int userId, int reportId, Dictionary<string, string> parameters);
    Task<ServiceResponse<ReportStats>> GetReportStatsAsync();
}