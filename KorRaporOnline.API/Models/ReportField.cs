using System.Text.Json.Serialization;

namespace KorRaporOnline.API.Models
{
    public class ReportField
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public bool Visible { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public AggregationType? Aggregation { get; set; }
    }
}