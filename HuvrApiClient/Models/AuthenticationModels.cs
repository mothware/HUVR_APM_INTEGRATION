using System.Text.Json.Serialization;

namespace HuvrApiClient.Models
{
    /// <summary>
    /// Request model for obtaining an access token
    /// </summary>
    public class TokenRequest
    {
        [JsonPropertyName("client_id")]
        public string ClientId { get; set; } = string.Empty;

        [JsonPropertyName("client_secret")]
        public string ClientSecret { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response model for access token
    /// </summary>
    public class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; } = string.Empty;

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        /// <summary>
        /// Calculated expiration time
        /// </summary>
        public DateTime ExpiresAt { get; set; }
    }
}
