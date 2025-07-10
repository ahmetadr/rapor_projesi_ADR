using System;

namespace KorRaporOnline.API.Models
{
    public class ReportStats
    {
        public int TotalReports { get; set; }
        public int ActiveReports { get; set; }
        public int PublicReports { get; set; }
        public int PrivateReports { get; set; }
        public DateTime LastReportCreated { get; set; }
        public int TotalExecutions { get; set; }
        public double AverageExecutionTime { get; set; }
    }
}