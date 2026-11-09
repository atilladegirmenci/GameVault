using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using GameDBWebApi.DTOs.Game;
using GameDBWebApi.Constants;
using GameDBWebApi.Entites;
using GameDBWebApi.Exceptions;
using Polly.CircuitBreaker;
using Polly.Timeout;

namespace GameDBWebApi.Services
{
    public sealed class IGDBApiService : IGameCatalogService
    {
        private readonly HttpClient _httpClient;
        private readonly ITwitchTokenService _tokenService;
        private readonly ILogger<IGDBApiService> _logger;
        private readonly string _clientId;

        public IGDBApiService(
            HttpClient httpClient,
            ITwitchTokenService tokenService,
            IConfiguration configuration,
            ILogger<IGDBApiService> logger)
        {
            _httpClient = httpClient;
            _tokenService = tokenService;
            _logger = logger;
            _clientId = configuration["IGDB:ClientId"]
                ?? throw new InvalidOperationException(ErrorMessages.External.IgdbClientIdNotConfigured);
        }

        public async Task<Game?> GetGameByCatalogIdAsync(int catalogId, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching catalog game {CatalogId} from IGDB.", catalogId);
            var games = await QueryGamesAsync(
                $"where id = {catalogId}; fields id,name,cover.image_id,first_release_date,aggregated_rating;",
                cancellationToken);

            var game = games.FirstOrDefault();
            if (game is null)
            {
                _logger.LogWarning("IGDB returned no game for catalog ID {CatalogId}.", catalogId);
            }

            return game is null ? null : MapGame(game);
        }

        public async Task<GameDetailsResponseDto> GetGameDetailsByCatalogIdAsync(
            int catalogId,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching catalog game details {CatalogId} from IGDB.", catalogId);
            var games = await QueryGamesAsync(  //igdb /games endpoint always returns a a json array
                $"where id = {catalogId}; fields id,name,summary,cover.image_id,first_release_date,aggregated_rating,genres.name,involved_companies.company.name,platforms.name;",
                cancellationToken);

            var game = games.FirstOrDefault()
                ?? throw new NotFoundException(ErrorMessages.External.IgdbGameNotFound);

            return new GameDetailsResponseDto
            {
                CatalogId = game.Id,
                Title = game.Name ?? string.Empty,
                Description = game.Summary ?? string.Empty,
                ImageUrl = GetImageUrl(game.Cover?.ImageId),
                Released = ToDateTime(game.FirstReleaseDate),
                Score = ToScore(game.AggregatedRating),
                Genres = game.Genres?.Select(x => x.Name).Where(x => x is not null).Cast<string>().ToList() ?? [],
                Developers = game.InvolvedCompanies?.Select(x => x.Company?.Name).Where(x => x is not null).Cast<string>().ToList() ?? [],
                Tags = [],
                Publishers = [],
                IsInMyLibrary = false,
                MyStatus = null
            };
        }

        public async Task<PaginatedFeedResponse> GetGamesByCatalogAsync(
            int page,
            int pageSize,
            string? keyword,
            string ordering,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Fetching IGDB catalog page {Page} with page size {PageSize}. Search: {HasSearchTerm}.",
                page,
                pageSize,
                !string.IsNullOrWhiteSpace(keyword));
            var offset = (page - 1) * pageSize;
            var where = string.IsNullOrWhiteSpace(keyword)
                ? string.Empty
                : $" where name ~ *\"{EscapeQueryValue(keyword)}\"*;";
            var sort = ordering.StartsWith("-", StringComparison.Ordinal) ? "desc" : "asc";
            var query = $"fields id,name,cover.image_id,aggregated_rating;{where} sort first_release_date {sort}; limit {pageSize}; offset {offset};";
            var games = await QueryGamesAsync(query, cancellationToken);

            return new PaginatedFeedResponse
            {
                TotalCount = offset + games.Count,
                CurrentPage = page,
                PageSize = pageSize,
                Games = games.Select(game => new GameCardDto
                {
                    CatalogId = game.Id,
                    Title = game.Name ?? string.Empty,
                    ImageUrl = GetImageUrl(game.Cover?.ImageId),
                    Score = ToScore(game.AggregatedRating)
                }).ToList()
            };
        }

        private async Task<List<IGDBGameResponse>> QueryGamesAsync(
            string query,
            CancellationToken cancellationToken)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, "games");
                request.Headers.Authorization = new AuthenticationHeaderValue(
                    "Bearer",
                    await _tokenService.GetAccessTokenAsync(cancellationToken));
                request.Headers.Add("Client-ID", _clientId);
                request.Content = new StringContent(query);

                using var response = await _httpClient.SendAsync(request, cancellationToken);
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("IGDB returned 404 for the games query.");
                    return [];
                }

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("IGDB request failed with status code {StatusCode}.", response.StatusCode);
                    throw new ExternalServiceException(ErrorMessages.External.IgdbRequestFailed);
                }

                var games = await response.Content.ReadFromJsonAsync<List<IGDBGameResponse>>(cancellationToken);
                if (games is null)
                {
                    _logger.LogError("IGDB returned an empty response body.");
                    throw new ExternalServiceException(ErrorMessages.External.IgdbResponseInvalid);
                }

                _logger.LogDebug("IGDB returned {GameCount} games.", games.Count);
                return games;
            }
            catch (BrokenCircuitException ex)
            {
                _logger.LogWarning(ex, "IGDB circuit is open.");
                throw new ExternalServiceUnavailableException(ErrorMessages.External.IgdbServiceTemporarilyUnavailable);
            }
            catch (TimeoutRejectedException ex)
            {
                _logger.LogWarning(ex, "IGDB request timed out.");
                throw new ExternalServiceUnavailableException(ErrorMessages.External.IgdbServiceTimeout);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "IGDB request failed.");
                throw new ExternalServiceException(ErrorMessages.External.IgdbServiceUnreachable);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "IGDB returned a response that could not be deserialized.");
                throw new ExternalServiceException(ErrorMessages.External.IgdbResponseInvalid);
            }
        }

        private static Game MapGame(IGDBGameResponse game) => new()
        {
            CatalogId = game.Id,
            Title = game.Name ?? string.Empty,
            ImageUrl = GetImageUrl(game.Cover?.ImageId),
            ReleaseDate = ToDateTime(game.FirstReleaseDate),
            Score = ToScore(game.AggregatedRating)
        };

        private static string GetImageUrl(string? imageId) =>
            string.IsNullOrWhiteSpace(imageId)
                ? string.Empty
                : $"https://images.igdb.com/igdb/image/upload/t_cover_big/{imageId}.jpg";

        private static DateTime? ToDateTime(long? unixSeconds) =>
            unixSeconds.HasValue
                ? DateTimeOffset.FromUnixTimeSeconds(unixSeconds.Value).UtcDateTime
                : null;

        private static short ToScore(double? score) =>
            (short)Math.Clamp((int)Math.Round(score ?? 0), 0, short.MaxValue);

        private static string EscapeQueryValue(string value) =>
            value.Trim().Replace("\\", "\\\\").Replace("\"", "\\\"");

        private sealed class IGDBGameResponse
        {
            [JsonPropertyName("id")] public int Id { get; set; }
            [JsonPropertyName("name")] public string? Name { get; set; }
            [JsonPropertyName("summary")] public string? Summary { get; set; }
            [JsonPropertyName("cover")] public IGDBCover? Cover { get; set; }
            [JsonPropertyName("first_release_date")] public long? FirstReleaseDate { get; set; }
            [JsonPropertyName("aggregated_rating")] public double? AggregatedRating { get; set; }
            [JsonPropertyName("genres")] public List<IGDBNamedItem>? Genres { get; set; }
            [JsonPropertyName("involved_companies")] public List<IGDBInvolvedCompany>? InvolvedCompanies { get; set; }
        }

        private sealed class IGDBCover
        {
            [JsonPropertyName("image_id")] public string? ImageId { get; set; }
        }

        private sealed class IGDBInvolvedCompany
        {
            [JsonPropertyName("company")] public IGDBNamedItem? Company { get; set; }
        }

        private sealed class IGDBNamedItem
        {
            [JsonPropertyName("name")] public string? Name { get; set; }
        }
    }
}
