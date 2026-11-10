using FluentAssertions;
using GameDBWebApi.Configuration;
using GameDBWebApi.Exceptions;
using GameDBWebApi.Services;
using Microsoft.Extensions.Options;
using GameDBWebApi.Tests;

namespace GameDBWebApi.Tests.Integration.ExternalApis;

public class MailKitEmailServiceTests
{
    [Fact]
    [Trait("Category", "Integration")]
    public async Task WhenSmtpIsUnavailableThrowsExternalServiceException()
    {
        var settings = Options.Create(new EmailSettings
        {
            SmtpHost = "127.0.0.1",
            SmtpPort = 1,
            EnableSsl = false,
            FromName = "GameVault",
            FromAddress = "from@example.com",
            Username = "user",
            Password = "password"
        });
        var service = new MailKitEmailService(settings, TestDoubles.Logger<MailKitEmailService>());

        await service.Invoking(x => x.SendVerificationEmailAsync(
                "to@example.com", "User", "https://example.com/verify"))
            .Should().ThrowAsync<ExternalServiceException>();
    }
}
