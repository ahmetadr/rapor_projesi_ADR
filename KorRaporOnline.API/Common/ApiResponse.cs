using System;

namespace KorRaporOnline.API.Common
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public DateTime Timestamp { get; set; }
        public string TraceId { get; set; }

        public static ApiResponse<T> CreateSuccess(T data, string message = null)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message ?? "Operation completed successfully",
                Data = data,
                Timestamp = DateTime.UtcNow,
                TraceId = Guid.NewGuid().ToString()
            };
        }

        public static ApiResponse<T> CreateError(string message, T data = default)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Data = data,
                Timestamp = DateTime.UtcNow,
                TraceId = Guid.NewGuid().ToString()
            };
        }
    }
}