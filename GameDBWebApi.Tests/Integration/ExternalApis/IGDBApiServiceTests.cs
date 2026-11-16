using FluentAssertions;
using GameDBWebApi.DTOs.Game;
using GameDBWebApi.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using GameDBWebApi.Tests;

namespace GameDBWebApi.Tests.Integration.ExternalApis;

public class IGDBApiServiceTests
{
    [Fact]
    [Trait("Category", "Integration")]
    public async Task SendsBearerAndMapsDetails()
    {
        var handler = new StubHttpMessageHandler((_, _) =>
            Task.FromResult(ExternalApiTestSupport.JsonResponse(
                """[{"id":42,"name":"Example","summary":"Summary","aggregated_rating":88.4,"cover":{"image_id":"cover"}}]""")));
        var token = new Mock<ITwitchTokenService>();
        token.Setup(x => x.GetAccessTokenAsync(It.IsAny<CancellationToken>())).ReturnsAsync("token");
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["IGDB:ClientId"] = "client" }).Build();
        var service = new IGDBApiService(
            new HttpClient(handler) { BaseAddress = new Uri("https://api.igdb.io/v4/") },
            token.Object,
            configuration,
            TestDoubles.Logger<IGDBApiService>());

        var result = await service.GetGameDetailsByCatalogIdAsync(42, CancellationToken.None);

        result.Title.Should().Be("Example");
        result.ImageUrl.Should().Contain("cover");
        handler.LastRequest!.Headers.Authorization!.Parameter.Should().Be("token");
        handler.LastRequest.Headers.GetValues("Client-ID").Single().Should().Be("client");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task BuildsIgdbSortQueryFromOrderingAndDirection()
    {
        var requestBodies = new List<string>();
        var handler = new StubHttpMessageHandler(async (request, cancellationToken) =>
        {
            requestBodies.Add(await request.Content!.ReadAsStringAsync(cancellationToken));
            return handlerResponse(requestBodies.Count);

            static HttpResponseMessage handlerResponse(int requestNumber) =>
                requestNumber == 1
                    ? ExternalApiTestSupport.JsonResponse("""[{"game_id":42,"value":1.2}]""")
                    : ExternalApiTestSupport.JsonResponse("""[{"id":42,"name":"Example","aggregated_rating":88.4}]""");
        });
        var token = new Mock<ITwitchTokenService>();
        token.Setup(x => x.GetAccessTokenAsync(It.IsAny<CancellationToken>())).ReturnsAsync("token");
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["IGDB:ClientId"] = "client" }).Build();
        var service = new IGDBApiService(
            new HttpClient(handler) { BaseAddress = new Uri("https://api.igdb.io/v4/") },
            token.Object,
            configuration,
            TestDoubles.Logger<IGDBApiService>());

        await service.GetGamesByCatalogAsync(
            1,
            10,
            null,
            GameFeedOrdering.Popular,
            SortDirection.Ascending,
            CancellationToken.None);

        requestBodies.Should().HaveCount(2);
        requestBodies[0].Should().Contain("where popularity_type = 1;");
        requestBodies[0].Should().Contain("sort value asc;");
        requestBodies[1].Should().Contain("where id = (42) & category = 0 & version_parent = null;");
    }
}
