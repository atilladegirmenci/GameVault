using System.Net;
using System.Security.Claims;
using System.Threading.RateLimiting;
using FluentAssertions;
using GameDBWebApi.Configuration;
using Microsoft.AspNetCore.Http;

namespace GameDBWebApi.Tests.Unit.Configuration;

public class RateLimitingConfigurationTests
{
    [Fact]
    public async Task AuthPolicies_EnforceConfiguredLimitsPerIpAddress()
    {
        await AssertLimit(RateLimitingConfiguration.GetLoginPartition, 5);
        await AssertLimit(RateLimitingConfiguration.GetRegisterPartition, 3);
        await AssertLimit(RateLimitingConfiguration.GetResendVerificationPartition, 3);
        await AssertLimit(RateLimitingConfiguration.GetForgotPasswordPartition, 3);
        await AssertLimit(RateLimitingConfiguration.GetResetPasswordPartition, 5);
        await AssertLimit(RateLimitingConfiguration.GetRefreshTokenPartition, 30);
    }

    [Fact]
    public async Task ReviewPolicy_EnforcesLimitPerAuthenticatedUser()
    {
        using var limiter = PartitionedRateLimiter.Create<HttpContext, string>(
            RateLimitingConfiguration.GetReviewCreationPartition);
        var firstUser = CreateContext("10.0.0.1", 42);
        var secondUser = CreateContext("10.0.0.2", 99);

        for (var request = 0; request < 10; request++)
        {
            (await limiter.AcquireAsync(firstUser)).IsAcquired.Should().BeTrue();
        }

        (await limiter.AcquireAsync(firstUser)).IsAcquired.Should().BeFalse();
        (await limiter.AcquireAsync(secondUser)).IsAcquired.Should().BeTrue();
    }

    private static async Task AssertLimit(
        Func<HttpContext, RateLimitPartition<string>> partitionFactory,
        int permitLimit)
    {
        using var limiter = PartitionedRateLimiter.Create<HttpContext, string>(
            partitionFactory);
        var firstIp = CreateContext("10.0.0.1");
        var secondIp = CreateContext("10.0.0.2");

        for (var request = 0; request < permitLimit; request++)
        {
            (await limiter.AcquireAsync(firstIp)).IsAcquired.Should().BeTrue();
        }

        (await limiter.AcquireAsync(firstIp)).IsAcquired.Should().BeFalse();
        (await limiter.AcquireAsync(secondIp)).IsAcquired.Should().BeTrue();
    }

    private static DefaultHttpContext CreateContext(string ipAddress, int? userId = null)
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse(ipAddress);

        if (userId.HasValue)
        {
            context.User = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString())],
                "test"));
        }

        return context;
    }
}
