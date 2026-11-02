namespace GameDBWebApi.Exceptions;

public sealed class UnauthorizedException : BusinessException
{
    public UnauthorizedException(string message)
        : base(message, StatusCodes.Status401Unauthorized) {}
}