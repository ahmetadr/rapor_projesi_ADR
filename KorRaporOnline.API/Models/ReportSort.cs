using System.Text.Json.Serialization;

namespace KorRaporOnline.API.Models
{
    public class ReportSort
    {
        public string Field { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public SortDirection Direction { get; set; }
    }
}