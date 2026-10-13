namespace GameDBWebApi.Exceptions
{
    public class BadRequestException : BusinessException
    {
        public BadRequestException(string message) : base(message, StatusCodes.Status400BadRequest) { }

    }
}
