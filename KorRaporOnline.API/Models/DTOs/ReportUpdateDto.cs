using System.ComponentModel.DataAnnotations;

namespace KorRaporOnline.API.Models.DTOs
{
    public class ReportUpdateDto
    {
        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        public string QueryText { get; set; }
    }
}