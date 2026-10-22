namespace GameDBWebApi.Exceptions
{
    public class ExternalServiceException : BusinessException
    {
        public ExternalServiceException(string message) : base(message, StatusCodes.Status502BadGateway)
        {
        }
    }
}
