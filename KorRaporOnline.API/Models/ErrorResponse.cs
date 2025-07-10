using System;

namespace KorRaporOnline.API.Models
{
    public class ErrorResponse
    {
        public string TraceId { get; set; }
        public string Message { get; set; }
        public string Details { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}