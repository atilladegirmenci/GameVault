using GameDBWebApi.Exceptions;

namespace GameDBWebApi.MiddleWares
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                // only log 500 errors, as 400 errors are expected and handled by the application
                if (ex is not BusinessException)
                {
                    _logger.LogError(ex, "An unhandled critical system exception occurred."); 
                }

                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var (statusCode, message) = exception switch
            {
                BusinessException businessEx => 
                    (businessEx.StatusCode, businessEx.Message),

                UnauthorizedAccessException => 
                    (StatusCodes.Status401Unauthorized, "Unauthorized access."),

                _ => 
                    (StatusCodes.Status500InternalServerError, 
                        "An unexpected internal server error occurred. Please try again later.")
            };

            context.Response.StatusCode = statusCode;

            if (exception is BadRequestException { Errors: not null } badRequestEx)
            {
                return context.Response.WriteAsJsonAsync(new
                {
                    message,
                    statusCode,
                    errors = badRequestEx.Errors
                });
            }

            return context.Response.WriteAsJsonAsync(new
            {
                message,
                statusCode
            });
        }
    }
}
