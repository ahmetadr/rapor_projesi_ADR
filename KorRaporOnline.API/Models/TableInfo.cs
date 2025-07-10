using System.Text.Json.Serialization;

namespace KorRaporOnline.API.Models
{
    public class TableInfo
    {
        public string TableName { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public TableType TableType { get; set; }
    }
}