namespace GameDBWebApi.Exceptions
{
    public class ExternalServiceException : BusinessException
    {
        public ExternalServiceException(string message) : base(message, StatusCodes.Status502BadGateway) // upstream failure/invalid upstream response
        {
        }
    }

    public class ExternalServiceUnavailableException : BusinessException // temporary inability to serve the request, including timeout or open circuit
    {
        public ExternalServiceUnavailableException(string message) : base(message, StatusCodes.Status503ServiceUnavailable)
        {
        }
    }
}
