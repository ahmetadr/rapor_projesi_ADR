using System.Text.Json.Serialization;

namespace KorRaporOnline.API.Models
{
    public class ReportFilter
    {
        public string Field { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public FilterOperator Operator { get; set; }
        public string Value { get; set; }
        public bool Parameterized { get; set; }
    }
}