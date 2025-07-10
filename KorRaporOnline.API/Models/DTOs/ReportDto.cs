using System.ComponentModel.DataAnnotations;

namespace KorRaporOnline.API.Models.DTOs
{
    public class ReportDto
    {
        public int ReportID { get; set; }
        public string ReportName { get; set; }
        public string Description { get; set; }
        public string QueryText { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int UserId { get; set; }
    }

  
}