namespace TeachPortal.Models.Models
{
    public class Result<T>
    {
        public bool Success { get; init; }
        public string Message { get; init; } = string.Empty;
        public T? Data { get; init; }
        public int StatusCode { get; init; } = 200;

        public Result() { }

        public Result(bool success, string message, T? data = default, int statusCode = 200)
        {
            Success = success;
            Message = message;
            Data = data;
            StatusCode = statusCode;
        }
    }
}
