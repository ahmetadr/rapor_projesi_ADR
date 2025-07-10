using System;
using System.Text.Json.Serialization;

namespace KorRaporOnline.API.Models
{
    public class SavedReportModel
    {
        public int ReportID { get; set; }
        public int ConnectionID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastModified { get; set; }
        public bool IsPublic { get; set; }
        public string BaseObjectName { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TableType BaseObjectType { get; set; }
    }
}