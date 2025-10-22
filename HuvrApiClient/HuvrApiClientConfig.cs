namespace HuvrApiClient
{
    /// <summary>
    /// Configuration options for the HUVR API client
    /// </summary>
    public class HuvrApiClientConfig
    {
        /// <summary>
        /// HUVR Client ID (format: [email protected])
        /// </summary>
        public string ClientId { get; set; } = string.Empty;

        /// <summary>
        /// HUVR Client Secret
        /// </summary>
        public string ClientSecret { get; set; } = string.Empty;

        /// <summary>
        /// Base URL for the HUVR API (default: https://api.huvrdata.app)
        /// </summary>
        public string BaseUrl { get; set; } = "https://api.huvrdata.app";

        /// <summary>
        /// HTTP timeout in seconds (default: 30)
        /// </summary>
        public int TimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// Whether to automatically retry on 401 (token expiration) errors (default: true)
        /// </summary>
        public bool AutoRetryOnTokenExpiration { get; set; } = true;

        /// <summary>
        /// Buffer time before token expiration to trigger refresh in minutes (default: 5)
        /// </summary>
        public int TokenRefreshBufferMinutes { get; set; } = 5;

        /// <summary>
        /// Validates the configuration
        /// </summary>
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(ClientId))
                throw new InvalidOperationException("ClientId is required");

            if (string.IsNullOrWhiteSpace(ClientSecret))
                throw new InvalidOperationException("ClientSecret is required");

            if (string.IsNullOrWhiteSpace(BaseUrl))
                throw new InvalidOperationException("BaseUrl is required");

            if (TimeoutSeconds <= 0)
                throw new InvalidOperationException("TimeoutSeconds must be greater than 0");

            if (TokenRefreshBufferMinutes < 0 || TokenRefreshBufferMinutes > 60)
                throw new InvalidOperationException("TokenRefreshBufferMinutes must be between 0 and 60");
        }
    }
}
