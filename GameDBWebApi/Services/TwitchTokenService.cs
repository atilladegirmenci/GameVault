using System.Net.Http.Json;
using System.Text.Json.Serialization;
using GameDBWebApi.Exceptions;

namespace GameDBWebApi.Services
{
    public sealed class TwitchTokenService : ITwitchTokenService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TwitchTokenService> _logger;
        private readonly SemaphoreSlim _tokenLock = new(1, 1);
        private string? _accessToken;
        private DateTimeOffset _expiresAt;

        public TwitchTokenService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<TwitchTokenService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(_accessToken) &&
                _expiresAt > DateTimeOffset.UtcNow.AddMinutes(1))
            {
                return _accessToken;
            }

            await _tokenLock.WaitAsync(cancellationToken);
            try
            {
                if (!string.IsNullOrWhiteSpace(_accessToken) &&
                    _expiresAt > DateTimeOffset.UtcNow.AddMinutes(1))
                {
                    return _accessToken;
                }

                var clientId = _configuration["IGDB:ClientId"];
                var clientSecret = _configuration["IGDB:ClientSecret"];

                if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
                {
                    throw new InvalidOperationException("IGDB client credentials are not configured.");
                }

                using var request = new HttpRequestMessage(HttpMethod.Post, "oauth2/token")
                {
                    Content = new FormUrlEncodedContent(new Dictionary<string, string>
                    {
                        ["client_id"] = clientId,
                        ["client_secret"] = clientSecret,
                        ["grant_type"] = "client_credentials"
                    })
                };

                using var response = await _httpClient.SendAsync(request, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Twitch token request failed with status code {StatusCode}.", response.StatusCode);
                    throw new ExternalServiceException("Unable to authenticate with Twitch.");
                }

                var token = await response.Content.ReadFromJsonAsync<TwitchTokenResponse>(cancellationToken);
                if (token is null || string.IsNullOrWhiteSpace(token.AccessToken) || token.ExpiresIn <= 0)
                {
                    throw new ExternalServiceException("Twitch returned an invalid access token.");
                }

                _accessToken = token.AccessToken;
                _expiresAt = DateTimeOffset.UtcNow.AddSeconds(token.ExpiresIn);
                return _accessToken;
            }
            finally
            {
                _tokenLock.Release();
            }
        }

        private sealed record TwitchTokenResponse(
            [property: JsonPropertyName("access_token")] string AccessToken,
            [property: JsonPropertyName("expires_in")] int ExpiresIn,
            [property: JsonPropertyName("token_type")] string TokenType);
    }
}
