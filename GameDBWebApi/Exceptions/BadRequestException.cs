namespace GameDBWebApi.Exceptions
{
    public class BadRequestException : BusinessException
    {
        public IDictionary<string, string[]>? Errors { get; }
        public BadRequestException(string message) : base(message, StatusCodes.Status400BadRequest) { }

        public BadRequestException(string message, IDictionary<string, string[]> errors) : base(message, StatusCodes.Status400BadRequest)
        {
            Errors = errors;
        }

    }
}
