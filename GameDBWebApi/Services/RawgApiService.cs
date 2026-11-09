using GameDBWebApi.Constants;
using GameDBWebApi.DTOs.Game;
using GameDBWebApi.Entites;
using GameDBWebApi.Exceptions;
using Polly.CircuitBreaker;
using Polly.Timeout;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameDBWebApi.Services
{
    public class RawgApiService : IGameCatalogService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<RawgApiService> _logger;
        private readonly string _apiKey;
        public RawgApiService(IConfiguration conf, HttpClient httpClient, ILogger<RawgApiService> logger)
        {
            _configuration = conf;
            _httpClient = httpClient;
            _logger = logger;
            _apiKey = _configuration["RawgApiKey"]!;
        }
        public async Task<Game?> GetGameByRawgIdAsync(int rawgId, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"fetching game details for RawgId: {rawgId}");

            var requestUrl = $"https://api.rawg.io/api/games/{rawgId}?key={_apiKey}";
            var response = await SendRequestAsync(requestUrl, cancellationToken); //TODO: consider using the using block for the response to ensure it is disposed of properly.

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Game with RawgId {RawgId} was not found on RAWG API.", rawgId);
                throw new NotFoundException(string.Format(ErrorMessages.External.GameNotFoundTemplate, rawgId));
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("RAWG API request failed with status code: {StatusCode} for RawgId: {RawgId}", response.StatusCode, rawgId);
                throw new ExternalServiceException(string.Format(ErrorMessages.External.ExternalApiErrorFetchingGameDetailsTemplate, response.StatusCode));
            }

            var jsonString = await response.Content.ReadAsStringAsync(); //TODO: this method has a overload that accepts a cancellation token. i can update these methods.
            var rawgData = JsonSerializer.Deserialize<RawgGameResponse>(jsonString);

            if (rawgData == null)
            {
                return null;
            }

            return new Game
            {
                RawgId = rawgId,
                Title = rawgData.Name,
                ImageUrl = rawgData.BackgroundImage,
                ReleaseDate = rawgData.Released,
                Score = (short)(rawgData.Metacritic ?? 0) // Convert nullable int to short, defaulting to 0 if null
            };

        }
        public async Task<GameDetailsResponseDto> GetGameDetailsByRawgIdAsync(
            int rawgId,
            CancellationToken cancellationToken)
        {
            var requestUrl = $"https://api.rawg.io/api/games/{rawgId}?key={_apiKey}";

            var response = await SendRequestAsync(requestUrl, cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Game details with RawgId {RawgId} were not found on RAWG API.", rawgId);
                throw new NotFoundException(string.Format(ErrorMessages.External.GameDetailsNotFoundTemplate, rawgId));
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("RAWG API request failed with status code: {StatusCode} for RawgId: {RawgId}", response.StatusCode, rawgId);
                throw new ExternalServiceException(string.Format(ErrorMessages.External.ExternalApiErrorFetchingGameDetailsTemplate, response.StatusCode));
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            var rawgData = JsonSerializer.Deserialize<RawgGameDetailsResponse>(jsonString);

            if (rawgData == null)
            {
                throw new ExternalServiceException(string.Format(ErrorMessages.External.FailedToDeserializeGameDetailsTemplate, rawgId));
            }

            return new GameDetailsResponseDto
            {
                RawgId = rawgData.Id,
                Title = rawgData.Name,
                Description = rawgData.Description,
                ImageUrl = rawgData.BackgroundImage,
                Released = rawgData.Released,
                Score = (short)(rawgData.Metacritic ?? 0),

                // list of strings for genres, developers, tags, and publishers. If any of these are null, we will return an empty list.
                Genres = rawgData.Genres?.Select(g => g.Name).ToList() ?? new List<string>(),
                Developers = rawgData.Developers?.Select(d => d.Name).ToList() ?? new List<string>(),
                Tags = rawgData.Tags?.Select(t => t.Name).ToList() ?? new List<string>(),
                Publishers = rawgData.Publishers?.Select(p => p.Name).ToList() ?? new List<string>(),

                // user related information will be handled on gameservice, so we will return default values here.
                IsInMyLibrary = false,
                MyStatus = null
            };

        }

        public async Task<PaginatedFeedResponse> GetGamesAsync(
            int page,
            int pageSize,
            string? keyword,
            string ordering,
            CancellationToken cancellationToken)
        {
            var requestUrl = $"https://api.rawg.io/api/games?key={_apiKey}&page={page}&page_size={pageSize}&ordering={ordering}";

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var encodedKeyword = Uri.EscapeDataString(keyword.Trim());
                requestUrl += $"&search={encodedKeyword}&search_precise=true&search_exact=true";

                _logger.LogInformation("Searching games from RAWG. Page: {Page}, Keyword: {Keyword} ", page, keyword);
            }
            else
            {
                _logger.LogInformation("Fetching game feed from RAWG. Page: {Page}, Ordering: {Ordering} ", page, ordering);
            }

            var response = await SendRequestAsync(requestUrl, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {

                _logger.LogWarning("RAWG API request failed with status code: {StatusCode}. URL: {Url}", response.StatusCode, requestUrl);
                throw new ExternalServiceException(string.Format(ErrorMessages.External.ExternalApiErrorFetchingFeedTemplate, response.StatusCode));

            }

            var jsonString = await response.Content.ReadAsStringAsync();
            var rawgData = JsonSerializer.Deserialize<RawgListResponse>(jsonString);

            if (rawgData == null || rawgData.Results == null || !rawgData.Results.Any())
            {
                return new PaginatedFeedResponse(); //empty if nothing found
            }

            var games = rawgData.Results.Select(g => new GameCardDto
            {
                RawgId = g.Id,
                Title = g.Name,
                ImageUrl = g.BackgroundImage,
                Score = (short)(g.Metacritic ?? 0) // Convert nullable int to short, defaulting to 0 if null
            }).ToList();

            return new PaginatedFeedResponse
            {
                TotalCount = rawgData.Count,
                CurrentPage = page,
                PageSize = pageSize,
                Games = games
            };

        }

        private async Task<HttpResponseMessage> SendRequestAsync(
            string requestUrl,
            CancellationToken cancellationToken)
        {
            try
            {
                return await _httpClient.GetAsync(requestUrl, cancellationToken);
            }
            catch (BrokenCircuitException ex)
            {
                _logger.LogWarning(ex, "RAWG circuit is open. Request was rejected: {Url}", requestUrl);
                throw new ExternalServiceUnavailableException(
                    ErrorMessages.External.RawgServiceTemporarilyUnavailable);
            }
            catch (TimeoutRejectedException ex)
            {
                _logger.LogWarning(ex, "RAWG request timed out after resilience policy: {Url}", requestUrl);
                throw new ExternalServiceUnavailableException(
                    ErrorMessages.External.RawgServiceTimeout);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "RAWG request failed after resilience policy: {Url}", requestUrl);
                throw new ExternalServiceException(
                    ErrorMessages.External.RawgServiceUnreachable);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested) // only catch if the cancellation was not requested by the caller. that means it was a timeout from the HttpClient.
            {
                _logger.LogWarning("RAWG request timed out: {Url}", requestUrl);
                throw new ExternalServiceUnavailableException(
                    ErrorMessages.External.RawgServiceTimeout);
            }
        }


        public class RawgListResponse
        {
            [JsonPropertyName("results")]
            public List<RawgGameResponse> Results { get; set; } = new List<RawgGameResponse>();

            [JsonPropertyName("count")]
            public int Count { get; set; }
        }
        public class RawgGameResponse
        {
            [JsonPropertyName("id")]
            public int Id { get; set; }

            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("background_image")]
            public string BackgroundImage { get; set; }

            [JsonPropertyName("released")]
            public DateTime? Released { get; set; }

            [JsonPropertyName("metacritic")]
            public int? Metacritic { get; set; } // Representing the Score
        }
        public class RawgGameDetailsResponse : RawgGameResponse
        {
            [JsonPropertyName("description_raw")]
            public string Description { get; set; }

            [JsonPropertyName("genres")]
            public List<RawgNamedItem> Genres { get; set; }

            [JsonPropertyName("tags")]
            public List<RawgNamedItem> Tags { get; set; }

            [JsonPropertyName("developers")]
            public List<RawgNamedItem> Developers { get; set; }

            [JsonPropertyName("publishers")]
            public List<RawgNamedItem> Publishers { get; set; }
        }
        public class RawgNamedItem
        {
            [JsonPropertyName("id")] public int Id { get; set; }
            [JsonPropertyName("name")] public string Name { get; set; }
        }


    }
}
