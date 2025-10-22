using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using GeApmClient.Models;

namespace GeApmClient
{
    /// <summary>
    /// Client for GE APM On-Premises API
    /// Provides authentication, TML measurement operations, and generic query capabilities
    /// </summary>
    public class GeApmClient : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private string? _authToken;
        private readonly SemaphoreSlim _authLock = new(1, 1);

        /// <summary>
        /// Initializes a new instance of the GeApmClient
        /// </summary>
        /// <param name="baseUrl">Base URL of the GE APM server (e.g., https://apm.company.com)</param>
        /// <param name="httpClient">Optional HttpClient for custom configuration</param>
        public GeApmClient(string baseUrl, HttpClient? httpClient = null)
        {
            _baseUrl = baseUrl?.TrimEnd('/') ?? throw new ArgumentNullException(nameof(baseUrl));
            _httpClient = httpClient ?? new HttpClient();
            _httpClient.BaseAddress = new Uri(_baseUrl);
        }

        #region Authentication

        /// <summary>
        /// Authenticates with GE APM and obtains a Meridium token
        /// </summary>
        /// <param name="username">GE APM username</param>
        /// <param name="password">GE APM password</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Login response containing the authentication token</returns>
        public async Task<LoginResponse> LoginAsync(
            string username,
            string password,
            CancellationToken cancellationToken = default)
        {
            var request = new LoginRequest
            {
                Username = username,
                Password = password
            };

            var response = await _httpClient.PostAsJsonAsync(
                "/meridium/api/login",
                request,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken)
                ?? throw new InvalidOperationException("Failed to parse login response");

            // Store the token for subsequent requests
            await _authLock.WaitAsync(cancellationToken);
            try
            {
                _authToken = loginResponse.Token;
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _authToken);
            }
            finally
            {
                _authLock.Release();
            }

            return loginResponse;
        }

        /// <summary>
        /// Checks if the client is authenticated
        /// </summary>
        public bool IsAuthenticated => !string.IsNullOrEmpty(_authToken);

        /// <summary>
        /// Ensures the client is authenticated before making API calls
        /// </summary>
        private void EnsureAuthenticated()
        {
            if (!IsAuthenticated)
            {
                throw new InvalidOperationException(
                    "Client is not authenticated. Call LoginAsync first.");
            }
        }

        #endregion

        #region Generic Query API

        /// <summary>
        /// Executes a generic OData query with path and parameters
        /// </summary>
        /// <typeparam name="T">Type to deserialize the response to</typeparam>
        /// <param name="path">OData path (e.g., "Thickness_Measurement_Location")</param>
        /// <param name="parameters">Query parameters for filtering, selecting, etc.</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Query result with items and pagination info</returns>
        public async Task<QueryResult<T>> QueryAsync<T>(
            string path,
            QueryParameters? parameters = null,
            CancellationToken cancellationToken = default)
        {
            EnsureAuthenticated();

            var url = $"/meridium/api/odata/{path.TrimStart('/')}";

            if (parameters != null)
            {
                var queryString = parameters.ToQueryString();
                if (!string.IsNullOrEmpty(queryString))
                {
                    url += $"?{queryString}";
                }
            }

            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var odataResponse = await response.Content.ReadFromJsonAsync<ODataResponse<T>>(cancellationToken)
                ?? throw new InvalidOperationException("Failed to parse OData response");

            return new QueryResult<T>
            {
                Items = odataResponse.Value,
                TotalCount = odataResponse.Count,
                NextLink = odataResponse.NextLink
            };
        }

        /// <summary>
        /// Executes a raw OData query and returns dynamic result
        /// </summary>
        /// <param name="path">OData path</param>
        /// <param name="parameters">Dictionary of query parameters</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Raw JSON string response</returns>
        public async Task<string> QueryRawAsync(
            string path,
            Dictionary<string, string>? parameters = null,
            CancellationToken cancellationToken = default)
        {
            EnsureAuthenticated();

            var url = $"/meridium/api/odata/{path.TrimStart('/')}";

            if (parameters != null && parameters.Count > 0)
            {
                var queryString = string.Join("&",
                    parameters.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
                url += $"?{queryString}";
            }

            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync(cancellationToken);
        }

        /// <summary>
        /// Gets the count of entities in a family
        /// </summary>
        /// <param name="familyKey">Family key (e.g., "Thickness_Measurement_Location")</param>
        /// <param name="filter">Optional filter expression</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Count of entities</returns>
        public async Task<int> GetCountAsync(
            string familyKey,
            string? filter = null,
            CancellationToken cancellationToken = default)
        {
            EnsureAuthenticated();

            var url = $"/meridium/api/odata/{familyKey}/$count";

            if (!string.IsNullOrWhiteSpace(filter))
            {
                url += $"?$filter={Uri.EscapeDataString(filter)}";
            }

            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var countString = await response.Content.ReadAsStringAsync(cancellationToken);
            return int.Parse(countString);
        }

        #endregion

        #region TML Operations

        /// <summary>
        /// Gets all Thickness Measurement Locations
        /// </summary>
        /// <param name="parameters">Optional query parameters</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of TML entities</returns>
        public async Task<QueryResult<ThicknessMeasurementLocation>> GetTmlsAsync(
            QueryParameters? parameters = null,
            CancellationToken cancellationToken = default)
        {
            return await QueryAsync<ThicknessMeasurementLocation>(
                "Thickness_Measurement_Location",
                parameters,
                cancellationToken);
        }

        /// <summary>
        /// Gets a specific TML by Entity Key
        /// </summary>
        /// <param name="entityKey">Entity Key of the TML</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>TML entity</returns>
        public async Task<ThicknessMeasurementLocation> GetTmlByKeyAsync(
            long entityKey,
            CancellationToken cancellationToken = default)
        {
            EnsureAuthenticated();

            var url = $"/meridium/api/odata/Thickness_Measurement_Location({entityKey})";
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<ThicknessMeasurementLocation>(cancellationToken)
                ?? throw new InvalidOperationException($"TML with key {entityKey} not found");
        }

        /// <summary>
        /// Gets a TML by TML ID
        /// </summary>
        /// <param name="tmlId">TML ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>TML entity (first match if multiple exist)</returns>
        public async Task<ThicknessMeasurementLocation?> GetTmlByIdAsync(
            string tmlId,
            CancellationToken cancellationToken = default)
        {
            var result = await QueryAsync<ThicknessMeasurementLocation>(
                "Thickness_Measurement_Location",
                new QueryParameters
                {
                    Filter = $"TML_ID eq '{tmlId}'",
                    Top = 1
                },
                cancellationToken);

            return result.Items.FirstOrDefault();
        }

        /// <summary>
        /// Gets all measurements for a specific TML
        /// </summary>
        /// <param name="tmlId">TML ID</param>
        /// <param name="parameters">Optional query parameters for filtering/sorting measurements</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of measurements for the TML</returns>
        public async Task<QueryResult<TmlMeasurement>> GetTmlMeasurementsAsync(
            string tmlId,
            QueryParameters? parameters = null,
            CancellationToken cancellationToken = default)
        {
            var queryParams = parameters ?? new QueryParameters();

            // Add filter for TML_ID
            if (string.IsNullOrWhiteSpace(queryParams.Filter))
            {
                queryParams.Filter = $"TML_ID eq '{tmlId}'";
            }
            else
            {
                queryParams.Filter = $"({queryParams.Filter}) and TML_ID eq '{tmlId}'";
            }

            return await QueryAsync<TmlMeasurement>(
                "TML_Measurement",
                queryParams,
                cancellationToken);
        }

        /// <summary>
        /// Gets measurements for multiple TMLs
        /// </summary>
        /// <param name="tmlIds">Array of TML IDs</param>
        /// <param name="parameters">Optional query parameters</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Dictionary mapping TML ID to its measurements</returns>
        public async Task<Dictionary<string, List<TmlMeasurement>>> GetMultipleTmlMeasurementsAsync(
            string[] tmlIds,
            QueryParameters? parameters = null,
            CancellationToken cancellationToken = default)
        {
            var result = new Dictionary<string, List<TmlMeasurement>>();

            // Build filter for all TML IDs
            var tmlFilters = tmlIds.Select(id => $"TML_ID eq '{id}'");
            var combinedFilter = string.Join(" or ", tmlFilters);

            var queryParams = parameters ?? new QueryParameters();
            if (string.IsNullOrWhiteSpace(queryParams.Filter))
            {
                queryParams.Filter = combinedFilter;
            }
            else
            {
                queryParams.Filter = $"({queryParams.Filter}) and ({combinedFilter})";
            }

            var measurements = await QueryAsync<TmlMeasurement>(
                "TML_Measurement",
                queryParams,
                cancellationToken);

            // Group by TML ID
            foreach (var measurement in measurements.Items)
            {
                if (measurement.TmlId == null) continue;

                if (!result.ContainsKey(measurement.TmlId))
                {
                    result[measurement.TmlId] = new List<TmlMeasurement>();
                }
                result[measurement.TmlId].Add(measurement);
            }

            return result;
        }

        /// <summary>
        /// Loads/creates a new TML measurement
        /// </summary>
        /// <param name="measurement">Measurement request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created measurement entity</returns>
        public async Task<TmlMeasurement> LoadTmlMeasurementAsync(
            TmlMeasurementRequest measurement,
            CancellationToken cancellationToken = default)
        {
            EnsureAuthenticated();

            // Create EntityDto for the measurement
            var entity = new EntityDto
            {
                EntityKey = 0, // Must be 0 for INSERT
                FamilyKey = "TML_Measurement",
                Properties = new Dictionary<string, object?>
                {
                    { "TML_ID", measurement.TmlId },
                    { "Measurement_Date", measurement.MeasurementDate },
                    { "Measured_Thickness", measurement.MeasuredThickness },
                    { "Measurement_Taken_By", measurement.MeasurementTakenBy },
                    { "Inspection_ID", measurement.InspectionId },
                    { "Reading_Number", measurement.ReadingNumber },
                    { "Temperature", measurement.Temperature },
                    { "Comments", measurement.Comments },
                    { "Status", measurement.Status },
                    { "Created_Date", DateTime.UtcNow }
                }
            };

            var response = await _httpClient.PostAsJsonAsync(
                "/meridium/api/odata/TML_Measurement",
                entity,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<TmlMeasurement>(cancellationToken)
                ?? throw new InvalidOperationException("Failed to create TML measurement");
        }

        /// <summary>
        /// Loads/creates multiple TML measurements in batch
        /// </summary>
        /// <param name="measurements">List of measurement requests</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of created measurements with success/failure status</returns>
        public async Task<List<(TmlMeasurementRequest Request, TmlMeasurement? Result, Exception? Error)>>
            LoadTmlMeasurementsBatchAsync(
                List<TmlMeasurementRequest> measurements,
                CancellationToken cancellationToken = default)
        {
            var results = new List<(TmlMeasurementRequest, TmlMeasurement?, Exception?)>();

            foreach (var measurement in measurements)
            {
                try
                {
                    var result = await LoadTmlMeasurementAsync(measurement, cancellationToken);
                    results.Add((measurement, result, null));
                }
                catch (Exception ex)
                {
                    results.Add((measurement, null, ex));
                }
            }

            return results;
        }

        /// <summary>
        /// Updates an existing TML measurement
        /// </summary>
        /// <param name="entityKey">Entity Key of the measurement to update</param>
        /// <param name="updates">Dictionary of fields to update</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated measurement entity</returns>
        public async Task<TmlMeasurement> UpdateTmlMeasurementAsync(
            long entityKey,
            Dictionary<string, object?> updates,
            CancellationToken cancellationToken = default)
        {
            EnsureAuthenticated();

            var entity = new
            {
                Properties = updates
            };

            var content = new StringContent(
                JsonSerializer.Serialize(entity),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PatchAsync(
                $"/meridium/api/odata/TML_Measurement({entityKey})",
                content,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<TmlMeasurement>(cancellationToken)
                ?? throw new InvalidOperationException("Failed to update TML measurement");
        }

        #endregion

        #region Entity Operations

        /// <summary>
        /// Creates a new entity in any family
        /// </summary>
        /// <param name="entity">Entity DTO with family key and properties</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created entity</returns>
        public async Task<EntityDto> CreateEntityAsync(
            EntityDto entity,
            CancellationToken cancellationToken = default)
        {
            EnsureAuthenticated();

            if (entity.EntityKey != 0)
            {
                throw new ArgumentException("EntityKey must be 0 for INSERT operations", nameof(entity));
            }

            var response = await _httpClient.PostAsJsonAsync(
                $"/meridium/api/odata/{entity.FamilyKey}",
                entity,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<EntityDto>(cancellationToken)
                ?? throw new InvalidOperationException("Failed to create entity");
        }

        /// <summary>
        /// Updates an existing entity (partial update)
        /// </summary>
        /// <param name="familyKey">Family key</param>
        /// <param name="entityKey">Entity key</param>
        /// <param name="updates">Dictionary of fields to update</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated entity</returns>
        public async Task<EntityDto> UpdateEntityAsync(
            string familyKey,
            long entityKey,
            Dictionary<string, object?> updates,
            CancellationToken cancellationToken = default)
        {
            EnsureAuthenticated();

            var entity = new
            {
                Properties = updates
            };

            var content = new StringContent(
                JsonSerializer.Serialize(entity),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PatchAsync(
                $"/meridium/api/odata/{familyKey}({entityKey})",
                content,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<EntityDto>(cancellationToken)
                ?? throw new InvalidOperationException("Failed to update entity");
        }

        /// <summary>
        /// Deletes an entity
        /// </summary>
        /// <param name="familyKey">Family key</param>
        /// <param name="entityKey">Entity key</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task DeleteEntityAsync(
            string familyKey,
            long entityKey,
            CancellationToken cancellationToken = default)
        {
            EnsureAuthenticated();

            var response = await _httpClient.DeleteAsync(
                $"/meridium/api/odata/{familyKey}({entityKey})",
                cancellationToken);

            response.EnsureSuccessStatusCode();
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            _httpClient?.Dispose();
            _authLock?.Dispose();
        }

        #endregion
    }
}
