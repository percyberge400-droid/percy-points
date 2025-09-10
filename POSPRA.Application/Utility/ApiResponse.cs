namespace POSPRA.Application.Utility
{
    public class ApiResponse<T>
    {
        public string StatusCode { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public object Errors { get; set; } // can be list/dictionary/null

        public ApiResponse(string statusCode, string message, T data = default, object errors = null)
        {
            StatusCode = statusCode;
            Message = message;
            Data = data;
            Errors = errors;
        }
    }
}
