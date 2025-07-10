using Azure;
using KorRaporOnline.API.Data;
using KorRaporOnline.API.Helpers;
using KorRaporOnline.API.Models;
using KorRaporOnline.API.Models.Common;
using KorRaporOnline.API.Models.DTOs;
using KorRaporOnline.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace KorRaporOnline.API.Services
{
    public class ReportService : IReportService
    {
        private readonly AppDbContext _context;
        private readonly IDatabaseService _databaseService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ReportService> _logger;

        public ReportService(
            AppDbContext context,
            IDatabaseService databaseService,
            IHttpContextAccessor httpContextAccessor,
            ILogger<ReportService> logger)
        {
            _context = context;
            _databaseService = databaseService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                throw new UnauthorizedAccessException("User is not authenticated or ID is invalid");
            }
            return userId;
        }

        public async Task<ServiceResponse<bool>> ValidateQueryAsync(string query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return new ServiceResponse<bool> { Success = false, Message = "Query cannot be empty" };
                }

                if (!SqlQueryValidator.IsValidSelectQuery(query))
                {
                    return new ServiceResponse<bool> { Success = false, Message = "Invalid query format" };
                }

                return new ServiceResponse<bool> { Success = true, Data = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Query validation failed");
                return new ServiceResponse<bool> { Success = false, Message = ex.Message };
            }
        }

        public async Task<ServiceResponse<ReportDto>> GetReport(int userId, int reportId)
        {
            try
            {
                var report = await _context.SavedReports
                    .FirstOrDefaultAsync(r => r.UserID == userId && r.SavedReportID == reportId);

                if (report == null)
                {
                    return new ServiceResponse<ReportDto> { Success = false, Message = "Report not found" };
                }

                var reportDto = new ReportDto
                {
                    ReportID = report.SavedReportID,
                    ReportName = report.ReportName,
                    Description = report.Description,
                    QueryText = report.QueryDefinition,
                    CreatedAt = report.CreatedAt,
                    UpdatedAt = report.LastModified,
                    UserId = report.UserID
                };

                return new ServiceResponse<ReportDto> { Success = true, Data = reportDto };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving report");
                return new ServiceResponse<ReportDto> { Success = false, Message = ex.Message };
            }
        }

        public async Task<ServiceResponse<ReportDto>> SaveReport(int userId, SavedReportDto report, ReportDefinition definition)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var savedReport = new SavedReport
                {
                    UserID = userId,
                    ReportName = report.Title,
                    Description = report.Description,
                    QueryDefinition = definition.QueryText,
                    CreatedAt = DateTime.UtcNow,
                    LastModified = DateTime.UtcNow,
                    IsPublic = report.IsPublic,
                    ConnectionID = report.ConnectionID
                };

                _context.SavedReports.Add(savedReport);
                await _context.SaveChangesAsync();

                var reportDto = new ReportDto
                {
                    ReportID = savedReport.SavedReportID,
                    ReportName = savedReport.ReportName,
                    Description = savedReport.Description,
                    QueryText = savedReport.QueryDefinition,
                    CreatedAt = savedReport.CreatedAt,
                    UpdatedAt = savedReport.LastModified,
                    UserId = savedReport.UserID
                };

                await transaction.CommitAsync();
                return new ServiceResponse<ReportDto> { Success = true, Data = reportDto };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error saving report");
                return new ServiceResponse<ReportDto> { Success = false, Message = ex.Message };
            }
        }

        public async Task<ServiceResponse<bool>> DeleteReport(int userId, int reportId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var report = await _context.SavedReports
                    .FirstOrDefaultAsync(r => r.UserID == userId && r.SavedReportID == reportId);

                if (report == null)
                {
                    return new ServiceResponse<bool> { Success = false, Message = "Report not found" };
                }

                _context.SavedReports.Remove(report);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new ServiceResponse<bool> { Success = true, Data = true };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error deleting report");
                return new ServiceResponse<bool> { Success = false, Message = ex.Message };
            }
        }

        public async Task<ServiceResponse<object>> ExecuteReport(int userId, int reportId, Dictionary<string, string> parameters)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var report = await _context.SavedReports
                    .Include(r => r.ReportParameters)
                    .FirstOrDefaultAsync(r => r.UserID == userId && r.SavedReportID == reportId);

                if (report == null)
                    return new ServiceResponse<object> { Success = false, Message = "Report not found" };

                if (!SqlQueryValidator.IsValidSelectQuery(report.QueryDefinition))
                    return new ServiceResponse<object> { Success = false, Message = "Invalid query" };

                if (!await ValidateParameters(parameters, report.ReportParameters))
                    return new ServiceResponse<object> { Success = false, Message = "Invalid parameters" };

                var execution = new ReportExecution
                {
                    ReportId = reportId,
                    UserId = userId,
                    StartTime = DateTime.UtcNow,
                    Status = "Running"
                };

                _context.ReportExecutions.Add(execution);
                await _context.SaveChangesAsync();

                using var cts = new CancellationTokenSource(
                    TimeSpan.FromSeconds(ValidationConstants.MAX_EXECUTION_TIMEOUT_SECONDS));

                var result = await _databaseService.ExecuteQueryAsync(report.QueryDefinition);

                execution.EndTime = DateTime.UtcNow;
                execution.Status = "Completed";
                execution.ExecutionTime = (execution.EndTime.Value - execution.StartTime).TotalSeconds;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new ServiceResponse<object> { Success = true, Data = result };
            }
            catch (OperationCanceledException)
            {
                await transaction.RollbackAsync();
                return new ServiceResponse<object> { Success = false, Message = "Report execution timeout" };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error executing report {ReportId}", reportId);
                return new ServiceResponse<object> { Success = false, Message = "An error occurred while executing the report" };
            }
        }

        public async Task<ServiceResponse<ReportStats>> GetReportStatsAsync()
        {
            try
            {
                var stats = new ReportStats
                {
                    TotalReports = await _context.SavedReports.CountAsync(),
                    PublicReports = await _context.SavedReports.CountAsync(r => r.IsPublic),
                    PrivateReports = await _context.SavedReports.CountAsync(r => !r.IsPublic),
                    LastReportCreated = await _context.SavedReports
                        .OrderByDescending(r => r.CreatedAt)
                        .Select(r => r.CreatedAt)
                        .FirstOrDefaultAsync(),
                    TotalExecutions = await _context.ReportExecutions.CountAsync(),
                    AverageExecutionTime = await _context.ReportExecutions
                        .Where(e => e.Status == "Completed")
                        .AverageAsync(e => e.ExecutionTime ?? 0)
                };

                return new ServiceResponse<ReportStats> { Success = true, Data = stats };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting report statistics");
                return new ServiceResponse<ReportStats> { Success = false, Message = ex.Message };
            }
        }

        public async Task<ServiceResponse<IEnumerable<ReportDto>>> GetAllReportsAsync()
        {
            try
            {
                var reports = await _context.SavedReports
                    .Select(r => new ReportDto
                    {
                        ReportID = r.SavedReportID,
                        ReportName = r.ReportName,
                        Description = r.Description,
                        QueryText = r.QueryDefinition,
                        CreatedAt = r.CreatedAt,
                        UpdatedAt = r.LastModified,
                        UserId = r.UserID
                    })
                    .ToListAsync();

                return new ServiceResponse<IEnumerable<ReportDto>> { Success = true, Data = reports };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all reports");
                return new ServiceResponse<IEnumerable<ReportDto>> { Success = false, Message = ex.Message };
            }
        }

        public async Task<ServiceResponse<PaginatedResult<ReportDto>>> GetUserReportsAsync(int userId, int page, int pageSize)
        {
            try
            {
                var query = _context.SavedReports
                    .Where(r => r.UserID == userId)
                    .OrderByDescending(r => r.LastModified);

                var totalCount = await query.CountAsync();
                var items = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(r => new ReportDto
                    {
                        ReportID = r.SavedReportID,
                        ReportName = r.ReportName,
                        Description = r.Description,
                        QueryText = r.QueryDefinition,
                        CreatedAt = r.CreatedAt,
                        UpdatedAt = r.LastModified,
                        UserId = r.UserID
                    })
                    .ToListAsync();

                var paginatedResult = new PaginatedResult<ReportDto>
                {
                    Items = items,
                    TotalCount = totalCount,
                    PageSize = pageSize,
                    CurrentPage = page,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                };

                return new ServiceResponse<PaginatedResult<ReportDto>>
                {
                    Success = true,
                    Data = paginatedResult
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user reports");
                return new ServiceResponse<PaginatedResult<ReportDto>>
                {
                    Success = false,
                    Message = "Error retrieving reports"
                };
            }
        }

        private async Task<bool> ValidateParameters(Dictionary<string, string> parameters, ICollection<ReportParameter> reportParameters)
        {
            if (reportParameters == null) return true;

            foreach (var param in reportParameters.Where(p => p.IsRequired))
            {
                if (!parameters.ContainsKey(param.Name) || string.IsNullOrEmpty(parameters[param.Name]))
                    return false;
            }

            return true;
        }

        public async Task<ServiceResponse<ReportDto>> GetReportByIdAsync(int id)
        {
            try
            {
                var report = await _context.SavedReports
                    .Where(r => r.SavedReportID == id)
                    .Select(r => new ReportDto
                    {
                        ReportID = r.SavedReportID,
                        ReportName = r.ReportName,
                        Description = r.Description,
                        QueryText = r.QueryDefinition,
                        CreatedAt = r.CreatedAt,
                        UpdatedAt = r.LastModified,
                        UserId = r.UserID
                    })
                    .FirstOrDefaultAsync();

                if (report == null)
                    return new ServiceResponse<ReportDto> { Success = false, Message = "Report not found" };

                return new ServiceResponse<ReportDto> { Success = true, Data = report };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting report by id");
                return new ServiceResponse<ReportDto> { Success = false, Message = ex.Message };
            }
        }

        public async Task<ServiceResponse<IEnumerable<ReportDto>>> SearchReportsAsync(string searchTerm, int page, int pageSize)
        {
            try
            {
                var query = _context.SavedReports.AsQueryable();

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    searchTerm = searchTerm.ToLower();
                    query = query.Where(r =>
                        r.ReportName.ToLower().Contains(searchTerm) ||
                        r.Description.ToLower().Contains(searchTerm));
                }

                var reports = await query
                    .OrderByDescending(r => r.LastModified)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(r => new ReportDto
                    {
                        ReportID = r.SavedReportID,
                        ReportName = r.ReportName,
                        Description = r.Description,
                        QueryText = r.QueryDefinition,
                        CreatedAt = r.CreatedAt,
                        UpdatedAt = r.LastModified,
                        UserId = r.UserID
                    })
                    .ToListAsync();

                return new ServiceResponse<IEnumerable<ReportDto>> { Success = true, Data = reports };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching reports");
                return new ServiceResponse<IEnumerable<ReportDto>> { Success = false, Message = ex.Message };
            }
        }

        public async Task<ServiceResponse<ReportDto>> CreateReportAsync(ReportCreateDto createReportDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var userId = GetCurrentUserId();

                var report = new SavedReport
                {
                    ReportName = createReportDto.Title,
                    Description = createReportDto.Description,
                    QueryDefinition = createReportDto.QueryText,
                    UserID = userId,
                    CreatedAt = DateTime.UtcNow,
                    LastModified = DateTime.UtcNow,
                    IsPublic = false
                };

                await _context.SavedReports.AddAsync(report);
                await _context.SaveChangesAsync();

                var reportDto = new ReportDto
                {
                    ReportID = report.SavedReportID,
                    ReportName = report.ReportName,
                    Description = report.Description,
                    QueryText = report.QueryDefinition,
                    CreatedAt = report.CreatedAt,
                    UpdatedAt = report.LastModified,
                    UserId = report.UserID
                };

                await transaction.CommitAsync();
                return new ServiceResponse<ReportDto> { Success = true, Data = reportDto };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating report");
                return new ServiceResponse<ReportDto> { Success = false, Message = ex.Message };
            }
        }

        public async Task<ServiceResponse<ReportDto>> UpdateReportAsync(int id, ReportUpdateDto reportDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var report = await _context.SavedReports.FindAsync(id);
                if (report == null)
                    return new ServiceResponse<ReportDto> { Success = false, Message = "Report not found" };

                report.ReportName = reportDto.Title;
                report.Description = reportDto.Description;
                report.QueryDefinition = reportDto.QueryText;
                report.LastModified = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var updatedReportDto = new ReportDto
                {
                    ReportID = report.SavedReportID,
                    ReportName = report.ReportName,
                    Description = report.Description,
                    QueryText = report.QueryDefinition,
                    CreatedAt = report.CreatedAt,
                    UpdatedAt = report.LastModified,
                    UserId = report.UserID
                };

                await transaction.CommitAsync();
                return new ServiceResponse<ReportDto> { Success = true, Data = updatedReportDto };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error updating report");
                return new ServiceResponse<ReportDto> { Success = false, Message = ex.Message };
            }
        }

        public async Task<ServiceResponse<bool>> DeleteReportAsync(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var report = await _context.SavedReports.FindAsync(id);
                if (report == null)
                    return new ServiceResponse<bool> { Success = false, Message = "Report not found" };

                _context.SavedReports.Remove(report);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new ServiceResponse<bool> { Success = true, Data = true, Message = "Report deleted successfully" };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error deleting report");
                return new ServiceResponse<bool> { Success = false, Message = ex.Message };
            }
        }


        public async Task<ServiceResponse<IEnumerable<ReportDto>>> GetUserReports(int userId)
        {
            try
            {
                var reports = await _context.SavedReports
                    .Where(r => r.UserID == userId)
                    .OrderByDescending(r => r.LastModified)
                    .Select(r => new ReportDto
                    {
                        ReportID = r.SavedReportID,
                        ReportName = r.ReportName,
                        Description = r.Description,
                        QueryText = r.QueryDefinition,
                        CreatedAt = r.CreatedAt,
                        UpdatedAt = r.LastModified,
                        UserId = r.UserID
                    })
                    .ToListAsync();

                return new ServiceResponse<IEnumerable<ReportDto>>
                {
                    Success = true,
                    Data = reports
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user reports");
                return new ServiceResponse<IEnumerable<ReportDto>>
                {
                    Success = false,
                    Message = "Error retrieving reports"
                };
            }
        }

        // KorRaporOnline.API/Services/ReportService.cs
        public async Task<ServiceResponse<PaginatedResult<ReportDto>>> GetUserReportsPaginated(int userId, int page, int pageSize)
        {
            try
            {
                var query = _context.SavedReports.Where(r => r.UserID == userId);

                var totalCount = await query.CountAsync();
                var items = await query
                    .OrderByDescending(r => r.LastModified)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(r => new ReportDto
                    {
                        ReportID = r.SavedReportID,
                        ReportName = r.ReportName,
                        Description = r.Description,
                        QueryText = r.QueryDefinition,
                        CreatedAt = r.CreatedAt,
                        UpdatedAt = r.LastModified,
                        UserId = r.UserID
                    })
                    .ToListAsync();

                var paginatedResult = new PaginatedResult<ReportDto>
                {
                    Items = items,
                    TotalCount = totalCount,
                    PageSize = pageSize,
                    CurrentPage = page,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                };

                return new ServiceResponse<PaginatedResult<ReportDto>>
                {
                    Success = true,
                    Data = paginatedResult
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paginated user reports");
                return new ServiceResponse<PaginatedResult<ReportDto>>
                {
                    Success = false,
                    Message = "Error retrieving reports"
                };
            }
        }
    }


}