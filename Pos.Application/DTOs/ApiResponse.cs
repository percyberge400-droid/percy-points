namespace Pos.Application.DTOs
{
    /// <summary>
    /// Represents a standard API response wrapper.
    /// </summary>
    /// <typeparam name="T">The type of the data returned in the response.</typeparam>
    public class ApiResponse<T>
    {
        /// <summary>
        /// Gets or sets the status code of the response (e.g., "200", "400", etc.).
        /// </summary>
        public string StatusCode { get; set; }

        /// <summary>
        /// Gets or sets a human-readable message associated with the response.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the data returned in the response. Can be <c>null</c>.
        /// </summary>
        public T Data { get; set; }

        /// <summary>
        /// Gets or sets any errors associated with the response.
        /// Can be a list, dictionary, or <c>null</c>.
        /// </summary>
        public object Errors { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiResponse{T}"/> class.
        /// </summary>
        /// <param name="statusCode">The status code of the response.</param>
        /// <param name="message">A human-readable message for the response.</param>
        /// <param name="data">Optional data returned in the response.</param>
        /// <param name="errors">Optional errors associated with the response.</param>
        public ApiResponse(string statusCode, string message, T data = default, object errors = null)
        {
            StatusCode = statusCode;
            Message = message;
            Data = data;
            Errors = errors;
        }
    }
}