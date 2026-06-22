namespace LMS_SoulCode.Features.Common
{
    public class ApiResponse<T>
    {
        public int Code { get; set; }
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }

        public static ApiResponse<T> Success(T? data, string message = "Success")
        {
            return new ApiResponse<T>
            {
                Code = StatusCodes.Success,
                IsSuccess = true,
                Message = message,
                Data = data
            };
        }

        public static ApiResponse<T> Fail(string message, int code)
        {
            return new ApiResponse<T>
            {
                Code = code,
                IsSuccess = false,
                Message = message,
                Data = default
            };
        }
    }
}