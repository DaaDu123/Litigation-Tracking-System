namespace LTSFrontend.Core.Exceptions
{





    public class ApiException : Exception
    {
        public int? StatusCode { get; }
        public List<string > Errors { get; }

        public ApiException(string message, int? statusCode = null, List<string>? errors = null) : base(message)
        {
            StatusCode = statusCode;
            Errors = errors ?? new List<string>();
        }





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
