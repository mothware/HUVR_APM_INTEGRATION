using System.Text.Json.Serialization;

namespace HuvrApiClient.Models
{
    /// <summary>
    /// Represents an error response from the HUVR API
    /// </summary>
    public class HuvrApiError
    {
        [JsonPropertyName("detail")]
        public string? Detail { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("error")]
        public string? Error { get; set; }

        [JsonPropertyName("code")]
        public string? Code { get; set; }

        [JsonPropertyName("status")]
        public int? Status { get; set; }

        [JsonPropertyName("errors")]
        public Dictionary<string, List<string>>? Errors { get; set; }
    }

    /// <summary>
    /// Exception thrown when a HUVR API request fails
    /// </summary>
    public class HuvrApiException : Exception
    {
        public int StatusCode { get; }
        public HuvrApiError? ApiError { get; }
        public string? ResponseBody { get; }

        public HuvrApiException(string message, int statusCode, HuvrApiError? apiError = null, string? responseBody = null)
            : base(message)
        {
            StatusCode = statusCode;
            ApiError = apiError;
            ResponseBody = responseBody;
        }

        public override string ToString()
        {
            var baseMessage = base.ToString();
            if (ApiError != null)
            {
                baseMessage += $"\nAPI Error: {ApiError.Detail ?? ApiError.Message ?? ApiError.Error}";
                if (ApiError.Errors != null && ApiError.Errors.Any())
                {
                    baseMessage += "\nValidation Errors:";
                    foreach (var (field, errors) in ApiError.Errors)
                    {
                        baseMessage += $"\n  {field}: {string.Join(", ", errors)}";
                    }
                }
            }
            return baseMessage;
        }
    }
}
