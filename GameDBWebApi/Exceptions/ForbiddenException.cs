namespace GameDBWebApi.Exceptions
{
    public class ForbiddenException : BusinessException
    {
        public ForbiddenException(string message) : base(message, StatusCodes.Status403Forbidden)
        {
        }
    }
}
