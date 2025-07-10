using KorRaporOnline.API.Extensions;
using KorRaporOnline.API.Models;
using KorRaporOnline.API.Models.DTOs;
using KorRaporOnline.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using KorRaporOnline.API.Models.Common;


namespace KorRaporOnline.API.Controllers.V1
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        /*
        [HttpGet]
        [ResponseCache(Duration = @"{Configuration['CacheSettings:ReportListDuration']}")]
        public async Task<ActionResult<IEnumerable<ReportDto>>> GetAllReports()
        {
            var result = await _reportService.GetAllReportsAsync();
            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Data);
        }
        */
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ReportDto>> GetReport(int id)
        {
            var result = await _reportService.GetReportByIdAsync(id);
            if (!result.Success)
                return NotFound(result.Message);

            return Ok(result.Data);
        }


        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<ReportDto>>> SearchReports(
            [FromQuery] string searchTerm,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = ValidationConstants.DEFAULT_PAGE_SIZE)
        {
            var result = await _reportService.SearchReportsAsync(searchTerm, page, pageSize);
            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Data);
        }


        [HttpGet("user/{userId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ReportDto>>> GetUserReports(
      int userId,
      [FromQuery] int? page = null,
      [FromQuery] int? pageSize = null)
        {
            // Kullanıcı yetkisi kontrolü
            if (!User.IsInRole("Admin") && userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Forbid();

            // Sayfalama parametreleri verilmişse paginated versiyonu kullan
            if (page.HasValue && pageSize.HasValue)
            {
                var paginatedResult = await _reportService.GetUserReportsPaginated(userId, page.Value, pageSize.Value);
                if (!paginatedResult.Success)
                    return BadRequest(paginatedResult.Message);

                // Toplam sayfa ve kayıt bilgilerini header'a ekle
                Response.Headers.Add("X-Total-Count", paginatedResult.Data.TotalCount.ToString());
                Response.Headers.Add("X-Total-Pages", paginatedResult.Data.TotalPages.ToString());
                Response.Headers.Add("X-Current-Page", paginatedResult.Data.CurrentPage.ToString());
                Response.Headers.Add("X-Page-Size", paginatedResult.Data.PageSize.ToString());

                return Ok(paginatedResult.Data.Items);
            }

            // Sayfalama parametreleri verilmemişse tüm listeyi getir
            var result = await _reportService.GetUserReports(userId);
            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Data);
        }

        [HttpGet("stats")]
        [ResponseCache(Duration = 60)]
        public async Task<ActionResult<ReportStats>> GetReportStats()
        {
            var result = await _reportService.GetReportStatsAsync();
            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Data);
        }

        [HttpPost("validate")]
        public async Task<ActionResult<bool>> ValidateQuery([FromBody] string query)
        {
            var result = await _reportService.ValidateQueryAsync(query);
            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Data);
        }

        [HttpPost]
        public async Task<ActionResult<ReportDto>> CreateReport(ReportCreateDto createReportDto)
        {
            var result = await _reportService.CreateReportAsync(createReportDto);
            if (!result.Success)
                return BadRequest(result.Message);

            return CreatedAtAction(
                nameof(GetReport),
                new { id = result.Data.ReportID },
                result.Data
            );
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ReportDto>> UpdateReport(int id, ReportUpdateDto reportDto)
        {
            var result = await _reportService.UpdateReportAsync(id, reportDto);
            if (!result.Success)
                return NotFound(result.Message);

            return Ok(result.Data);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,ReportManager")]
        public async Task<ActionResult> DeleteReport(int id)
        {
            var result = await _reportService.DeleteReportAsync(id);
            if (!result.Success)
                return NotFound(result.Message);

            return NoContent();
        }

      

        [HttpGet("user/{userId}/report/{reportId}")]
        public async Task<ActionResult<ReportDto>> GetUserReport(int userId, int reportId)
        {
            var result = await _reportService.GetReport(userId, reportId);
            if (!result.Success)
                return NotFound(result.Message);

            return Ok(result.Data);
        }

        [HttpPost("user/{userId}/save")]
        public async Task<ActionResult<ReportDto>> SaveReport(int userId, [FromBody] SaveReportRequest request)
        {
            var result = await _reportService.SaveReport(userId, request.Report, request.Definition);
            if (!result.Success)
                return BadRequest(result.Message);

            return CreatedAtAction(
                nameof(GetReport),
                new { id = result.Data.ReportID },
                result.Data
            );
        }

        [HttpDelete("user/{userId}/report/{reportId}")]
        public async Task<ActionResult> DeleteUserReport(int userId, int reportId)
        {
            var result = await _reportService.DeleteReport(userId, reportId);
            if (!result.Success)
                return NotFound(result.Message);

            return NoContent();
        }

        [HttpPost("user/{userId}/report/{reportId}/execute")]
        public async Task<ActionResult> ExecuteReport(int userId, int reportId, [FromBody] Dictionary<string, string> parameters)
        {
            var result = await _reportService.ExecuteReport(userId, reportId, parameters);
            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Data);
        }
    }

    public class SaveReportRequest
    {
        public SavedReportDto Report { get; set; }
        public ReportDefinition Definition { get; set; }
    }
}