namespace KorRaporOnline.API.Models
{
    public class ServiceResult<T>
    {
        public bool Success { get; private set; }
        public string Message { get; private set; }
        public T Data { get; private set; }

        private ServiceResult(bool success, string message, T data)
        {
            Success = success;
            Message = message;
            Data = data;
        }

        public static ServiceResult<T> CreateSuccess(T data, string message = null)
        {
            return new ServiceResult<T>(true, message, data);
        }

        public static ServiceResult<T> CreateError(string message)
        {
            return new ServiceResult<T>(false, message, default);
        }
    }
}