namespace KorRaporOnline.API.Models
{
    public class ExecuteReportModel
    {
        public int ReportID { get; set; }
        public Dictionary<string, string> Parameters { get; set; }
    }
}