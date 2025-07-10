namespace KorRaporOnline.API.Models
{
    public class ReportParameter : BaseEntity
    {

        public int ParameterID { get; set; }
        public int SavedReportID { get; set; }
        public string Name { get; set; }
        public string DataType { get; set; }
        public string DefaultValue { get; set; }
        public bool IsRequired { get; set; }

        // Navigation property
        public virtual SavedReport SavedReport { get; set; }

       
        public int ReportID { get; set; }
        public string ParameterName { get; set; }
        public string ParameterType { get; set; }
         

        public virtual SavedReport Report { get; set; }
    }
}