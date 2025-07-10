namespace KorRaporOnline.API.Models.DTOs
{
    public class ReportCreateDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string QueryText { get; set; }
    }
}