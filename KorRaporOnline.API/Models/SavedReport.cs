using KorRaporOnline.API.Models;

public class SavedReport
{
    public int SavedReportID { get; set; }
    public string ReportName { get; set; }
    public string Description { get; set; }
    public string QueryDefinition { get; set; }
    public int UserID { get; set; }
    public int ConnectionID { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastModified { get; set; }
    public bool IsPublic { get; set; }

    // Navigation properties
    public virtual User User { get; set; }
    public virtual DatabaseConnection DatabaseConnection { get; set; }
    public virtual ICollection<ReportParameter> ReportParameters { get; set; }
    public virtual ICollection<ReportExecution> ReportExecutions { get; set; }
}