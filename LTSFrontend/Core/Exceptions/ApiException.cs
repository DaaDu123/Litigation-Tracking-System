namespace LTSFrontend.Core.Exceptions
{
    /// <summary>
    /// Thrown by ApiClient whenever the backend returns Success = false,
    /// or a non-2xx status code. Carries the message + validation errors
    /// coming back from LTSBackend's ApiResponse&lt;T&gt; envelope.
    /// </summary>
    public class ApiException : Exception
    {
        public int? StatusCode { get; }
        public List<string> Errors { get; }

        public ApiException(string message, int? statusCode = null, List<string>? errors = null) : base(message)
        {
            StatusCode = statusCode;
            Errors = errors ?? new List<string>();
        }

        /// <summary>
        /// Combines the main message with the individual field errors,
        /// good for showing in a single alert box.
        /// </summary>
        public string GetFullMessage()
        {
            if (Errors.Count == 0)
            {
                return Message;
            }

            return Message + Environment.NewLine + string.Join(Environment.NewLine, Errors);
        }
    }
}
