namespace KorRaporOnline.API.Models
{
    public class ServiceResponse<T>
    {


        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }


 

        public static ServiceResponse<T> CreateSuccess(T data, string message = null)
        {
            return new ServiceResponse<T>
            {
                Data = data,
                Success = true,
                Message = message
            };
        }

        public static ServiceResponse<T> CreateFailure(string message)
        {
            return new ServiceResponse<T>
            {
                Success = false,
                Message = message
            };
        }
    }
}