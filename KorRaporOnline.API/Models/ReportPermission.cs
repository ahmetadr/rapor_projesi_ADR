using System;

namespace KorRaporOnline.API.Models
{
    public class ReportPermission
    {
        public int PermissionID { get; set; }
        public int ReportID { get; set; }
        public int UserID { get; set; }
        public string PermissionType { get; set; }
        public DateTime GrantedAt { get; set; }

        // Navigation properties
        public virtual Report Report { get; set; }
        public virtual User User { get; set; }
    }
}