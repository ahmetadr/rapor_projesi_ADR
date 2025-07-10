using KorRaporOnline.API.Data;
using System;
using System.Collections.Generic;

namespace KorRaporOnline.API.Models
{
    public class Report
    {
        public int ReportID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string QueryTemplate { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation property
        public virtual ICollection<ReportPermission> ReportPermissions { get; set; }
    }
}