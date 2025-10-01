namespace Trippio.Core.Models.Common
{
    public class BaseResponse<T>
    {
        public int Code { get; set; } = 200;
        public string Message { get; set; } = "Success";
        public T? Data { get; set; }

        public static BaseResponse<T> Success(T data, string message = "Success")
        {
            return new BaseResponse<T>
            {
                Code = 200,
                Message = message,
                Data = data
            };
        }

        public static BaseResponse<T> Error(string message, int code = 400)
        {
            return new BaseResponse<T>
            {
                Code = code,
                Message = message,
                Data = default(T)
            };
        }

        public static BaseResponse<T> NotFound(string message = "Not found")
        {
            return new BaseResponse<T>
            {
                Code = 404,
                Message = message,
                Data = default(T)
            };
        }

        public static BaseResponse<T> ServerError(string message = "Internal server error")
        {
            return new BaseResponse<T>
            {
                Code = 500,
                Message = message,
                Data = default(T)
            };
        }
    }
}