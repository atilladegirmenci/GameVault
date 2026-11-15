using System.Globalization;
using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;

namespace GameDBWebApi.Configuration;

public static class RateLimitingConfiguration
{
    public const string AuthLoginPolicy = "AuthLogin";
    public const string AuthRegisterPolicy = "AuthRegister";
    public const string AuthResendVerificationPolicy = "AuthResendVerification";
    public const string AuthForgotPasswordPolicy = "AuthForgotPassword";
    public const string AuthResetPasswordPolicy = "AuthResetPassword";
    public const string AuthRefreshTokenPolicy = "AuthRefreshToken";
    public const string ReviewCreationPolicy = "ReviewCreation";

    public static IServiceCollection AddGameVaultRateLimiting(
        this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = async (context, cancellationToken) =>
            {
                if (context.Lease.TryGetMetadata(
                    MetadataName.RetryAfter,
                    out TimeSpan retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter =
                        Math.Max(1, (int)Math.Ceiling(retryAfter.TotalSeconds))
                            .ToString(CultureInfo.InvariantCulture);
                }

                context.HttpContext.Response.ContentType = "application/json";
                await context.HttpContext.Response.WriteAsJsonAsync(new
                {
                    message = "Too many requests. Please try again later.",
                    statusCode = StatusCodes.Status429TooManyRequests
                }, cancellationToken);
            };

            options.AddPolicy(AuthLoginPolicy, GetLoginPartition);
            options.AddPolicy(AuthRegisterPolicy, GetRegisterPartition);
            options.AddPolicy(AuthResendVerificationPolicy, GetResendVerificationPartition);
            options.AddPolicy(AuthForgotPasswordPolicy, GetForgotPasswordPartition);
            options.AddPolicy(AuthResetPasswordPolicy, GetResetPasswordPartition);
            options.AddPolicy(AuthRefreshTokenPolicy, GetRefreshTokenPartition);
            options.AddPolicy(ReviewCreationPolicy, GetReviewCreationPartition);
        });

        return services;
    }

    public static RateLimitPartition<string> GetLoginPartition(HttpContext context) =>
        CreateFixedWindowPartition(GetIpAddress(context), 5, TimeSpan.FromMinutes(1));

    public static RateLimitPartition<string> GetRegisterPartition(HttpContext context) =>
        CreateFixedWindowPartition(GetIpAddress(context), 3, TimeSpan.FromMinutes(10));

    public static RateLimitPartition<string> GetResendVerificationPartition(HttpContext context) =>
        CreateFixedWindowPartition(GetIpAddress(context), 3, TimeSpan.FromMinutes(15));

    public static RateLimitPartition<string> GetForgotPasswordPartition(HttpContext context) =>
        CreateFixedWindowPartition(GetIpAddress(context), 3, TimeSpan.FromMinutes(15));

    public static RateLimitPartition<string> GetResetPasswordPartition(HttpContext context) =>
        CreateFixedWindowPartition(GetIpAddress(context), 5, TimeSpan.FromMinutes(15));

    public static RateLimitPartition<string> GetRefreshTokenPartition(HttpContext context) =>
        CreateFixedWindowPartition(GetIpAddress(context), 30, TimeSpan.FromMinutes(1));

    public static RateLimitPartition<string> GetReviewCreationPartition(HttpContext context) =>
        CreateFixedWindowPartition(GetUserPartitionKey(context), 10, TimeSpan.FromHours(1));

    private static RateLimitPartition<string> CreateFixedWindowPartition(
        string partitionKey,
        int permitLimit,
        TimeSpan window)
    {
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = permitLimit,
                Window = window,
                QueueLimit = 0,
                AutoReplenishment = true
            });
    }

    private static string GetIpAddress(HttpContext context) =>
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

    private static string GetUserPartitionKey(HttpContext context)
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return string.IsNullOrWhiteSpace(userId)
            ? $"ip:{GetIpAddress(context)}"
            : $"user:{userId}";
    }
}
