using System.Net;
using System.Text;
using FluentAssertions;
using GameDBWebApi.Services;
using GameDBWebApi.Tests;
using Microsoft.Extensions.Configuration;

namespace GameDBWebApi.Tests.Integration.ExternalApis;

public class RawgApiServiceTests
{
    [Fact]
    [Trait("Category", "Integration")]
    public async Task MapsGameResponse()
    {
        var handler = new StubHttpMessageHandler((_, _) =>
            Task.FromResult(ExternalApiTestSupport.JsonResponse(
                """{"id":42,"name":"Example","background_image":"img","metacritic":91}""")));
        var service = new RawgApiService(
            new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["RawgApiKey"] = "key" }).Build(),
            new HttpClient(handler) { BaseAddress = new Uri("https://api.rawg.io/") },
            TestDoubles.Logger<RawgApiService>());

        var result = await service.GetGameByCatalogIdAsync(42, CancellationToken.None);

        result.Should().NotBeNull();
        result!.CatalogId.Should().Be(42);
        result.Title.Should().Be("Example");
        result.Score.Should().Be(91);
        handler.LastRequest!.RequestUri!.Query.Should().Contain("key=key");
    }
}
