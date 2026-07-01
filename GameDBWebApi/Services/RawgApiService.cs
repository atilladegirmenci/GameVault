using GameDBWebApi.DTOs.Game;
using GameDBWebApi.Entites;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameDBWebApi.Services
{
    public class RawgApiService : IRawgApiService
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
            _apiKey = _configuration["RawgApiKey"] ?? string.Empty; //TODO: DONT FORGET TO ADD THIS TO USER SETTINGS

            if (string.IsNullOrEmpty(_apiKey))
            {
                _logger.LogCritical("RAWG API Key is missing or not configured");
                throw new Exception("THERE IS A PROBLEM WITH RAWG API KEY");
            }

        }
        public async Task<Game?> GetGameDetailsByRawgIdAsync(int rawgId)
        {

            var requestUrl = $"https://api.rawg.io/api/games/{rawgId}?key={_apiKey}";
            var response = await _httpClient.GetAsync(requestUrl);
            _logger.LogInformation($"fetching game details for RawgId: {rawgId}");

            if (!response.IsSuccessStatusCode)
            {
                // Handle error response maybe 404 or 500. i can add different handling logic for different status codes???
                _logger.LogWarning("RAWG API request failed with status code: {StatusCode} for RawgId: {RawgId}", response.StatusCode, rawgId);
                return null;

            }

            var jsonString = await response.Content.ReadAsStringAsync();
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
        public async Task<PaginatedFeedResponse> GetGameFeedAsync(int page, int pageSize, string ordering)
        {
            var requestUrl = $"https://api.rawg.io/api/games?key={_apiKey}&page={page}&page_size={pageSize}&ordering={ordering}";

            _logger.LogInformation("Fetching game feed from RAWG. Page: {Page}, Ordering: {Ordering} ", page, ordering);

            var response = await _httpClient.GetAsync(requestUrl);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("RAWG API request failed with status code: {StatusCode} for game feed. Page: {Page}, Ordering: {Ordering}", response.StatusCode, page, ordering);
                throw new Exception($"RAWG API request failed with status code: {response.StatusCode}");
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            var rawgData = JsonSerializer.Deserialize<RawgListResponse>(jsonString);

            if (rawgData == null || !rawgData.Results.Any())
            {
                return new PaginatedFeedResponse(); //empty if nothing found
            }

            var feedGames = rawgData.Results.Select(g => new GameFeedDto
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
                Games = feedGames
            };
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
}
