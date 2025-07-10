// Mevcut Extensions klasörüne ekleyelim: KorRaporOnline.API/Extensions/ModelExtensions.cs
using KorRaporOnline.API.Models;
using KorRaporOnline.API.Models.DTOs;

namespace KorRaporOnline.API.Extensions
{
    public static class ModelExtensions
    {
        public static ReportDto ToDto(this SavedReport report)
        {
            return new ReportDto
            {
                ReportID = report.SavedReportID,
                ReportName = report.ReportName,
                Description = report.Description,
                QueryText = report.QueryDefinition,
                CreatedAt = report.CreatedAt,
                UpdatedAt = report.LastModified,
                UserId = report.UserID
            };
        }
    }
} 
