using FluentAssertions;
using GameDBWebApi.Services;
using Microsoft.Extensions.Configuration;
using GameDBWebApi.Tests;

namespace GameDBWebApi.Tests.Integration.ExternalApis;

public class TwitchTokenServiceTests
{
    [Fact]
    [Trait("Category", "Integration")]
    public async Task CachesSuccessfulToken()
    {
        var handler = new StubHttpMessageHandler((_, _) =>
            Task.FromResult(ExternalApiTestSupport.JsonResponse(
                """{"access_token":"abc","expires_in":3600,"token_type":"bearer"}""")));
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["IGDB:ClientId"] = "client",
            ["IGDB:ClientSecret"] = "secret"
        }).Build();
        var service = new TwitchTokenService(
            new HttpClient(handler) { BaseAddress = new Uri("https://id.twitch.tv/") },
            configuration,
            TestDoubles.Logger<TwitchTokenService>());

        var first = await service.GetAccessTokenAsync(CancellationToken.None);
        var second = await service.GetAccessTokenAsync(CancellationToken.None);

        first.Should().Be("abc");
        second.Should().Be("abc");
        handler.CallCount.Should().Be(1);
    }
}
