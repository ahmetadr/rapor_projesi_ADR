using System;

namespace KorRaporOnline.API.Models
{
    public class ScheduledReport
    {
        public int ScheduleID { get; set; }
        public int ReportID { get; set; }
        public int UserID { get; set; }
        public string Schedule { get; set; }
        public string EmailRecipients { get; set; }
        public string ExportFormat { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastRun { get; set; }

        // Navigation properties
        public virtual SavedReport SavedReport { get; set; }
        public virtual User User { get; set; }
    }
}