using System;

namespace KorRaporOnline.API.Models
{
    public class ReportExecution : BaseEntity
    {

        public int ExecutionID { get; set; }
        public int SavedReportID { get; set; }
        public int UserID { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string Status { get; set; }
        public string ErrorMessage { get; set; }

        // Navigation properties
        public virtual SavedReport SavedReport { get; set; }
        public virtual User User { get; set; }

        public int Id { get; set; }
        public int ReportId { get; set; }
        public int UserId { get; set; }
        
        public double? ExecutionTime { get; set; }
        
        public int? RowsAffected { get; set; }

        public virtual SavedReport Report { get; set; }
        
    }
}