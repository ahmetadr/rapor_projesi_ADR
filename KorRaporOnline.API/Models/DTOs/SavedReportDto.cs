namespace KorRaporOnline.API.Models.DTOs
{
    public class SavedReportDto
    {
        public int ReportID { get; set; }
        public int ConnectionID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsPublic { get; set; }
    }
}