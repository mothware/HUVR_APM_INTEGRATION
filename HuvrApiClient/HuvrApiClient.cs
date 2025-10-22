using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using HuvrApiClient.Models;

namespace HuvrApiClient
{
    /// <summary>
    /// Main client for interacting with the HUVR Data API
    /// </summary>
    public class HuvrApiClient : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private TokenResponse? _currentToken;
        private readonly SemaphoreSlim _tokenLock = new(1, 1);
        private const string BaseUrl = "https://api.huvrdata.app";

        /// <summary>
        /// Initializes a new instance of the HuvrApiClient
        /// </summary>
        /// <param name="clientId">HUVR Client ID (format: [email protected])</param>
        /// <param name="clientSecret">HUVR Client Secret</param>
        /// <param name="httpClient">Optional HttpClient for custom configuration</param>
        public HuvrApiClient(string clientId, string clientSecret, HttpClient? httpClient = null)
        {
            _clientId = clientId ?? throw new ArgumentNullException(nameof(clientId));
            _clientSecret = clientSecret ?? throw new ArgumentNullException(nameof(clientSecret));
            _httpClient = httpClient ?? new HttpClient();
            _httpClient.BaseAddress = new Uri(BaseUrl);
        }

        #region Authentication

        /// <summary>
        /// Obtains a new access token from the HUVR API
        /// </summary>
        public async Task<TokenResponse> ObtainAccessTokenAsync(CancellationToken cancellationToken = default)
        {
            var request = new TokenRequest
            {
                ClientId = _clientId,
                ClientSecret = _clientSecret
            };

            var response = await _httpClient.PostAsJsonAsync("/api/auth/obtain-access-token/", request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var token = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken);
            if (token != null)
            {
                token.ExpiresAt = DateTime.UtcNow.AddSeconds(token.ExpiresIn);
                _currentToken = token;
            }

            return token ?? throw new InvalidOperationException("Failed to obtain access token");
        }

        /// <summary>
        /// Ensures a valid access token is available, refreshing if necessary
        /// </summary>
        private async Task EnsureAuthenticatedAsync(CancellationToken cancellationToken = default)
        {
            await _tokenLock.WaitAsync(cancellationToken);
            try
            {
                if (_currentToken == null || DateTime.UtcNow >= _currentToken.ExpiresAt.AddMinutes(-5))
                {
                    await ObtainAccessTokenAsync(cancellationToken);
                }

                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Token", _currentToken!.AccessToken);
            }
            finally
            {
                _tokenLock.Release();
            }
        }

        #endregion

        #region Assets

        /// <summary>
        /// Lists all assets with optional filtering
        /// </summary>
        /// <param name="queryParams">Optional query parameters for filtering</param>
        public async Task<AssetListResponse> ListAssetsAsync(
            Dictionary<string, string>? queryParams = null,
            CancellationToken cancellationToken = default)
        {
            await EnsureAuthenticatedAsync(cancellationToken);
            var url = BuildUrl("/api/assets/", queryParams);
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<AssetListResponse>(cancellationToken)
                ?? new AssetListResponse();
        }

        /// <summary>
        /// Gets details for a specific asset
        /// </summary>
        /// <param name="assetId">The asset ID</param>
        public async Task<Asset> GetAssetAsync(string assetId, CancellationToken cancellationToken = default)
        {
            await EnsureAuthenticatedAsync(cancellationToken);
            var response = await _httpClient.GetAsync($"/api/assets/{assetId}/", cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Asset>(cancellationToken)
                ?? throw new InvalidOperationException("Asset not found");
        }

        /// <summary>
        /// Creates a new asset
        /// </summary>
        /// <param name="asset">Asset details</param>
        public async Task<Asset> CreateAssetAsync(AssetRequest asset, CancellationToken cancellationToken = default)
        {
            await EnsureAuthenticatedAsync(cancellationToken);
            var response = await _httpClient.PostAsJsonAsync("/api/assets/", asset, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Asset>(cancellationToken)
                ?? throw new InvalidOperationException("Failed to create asset");
        }

        /// <summary>
        /// Updates an asset (partial update using PATCH)
        /// </summary>
        /// <param name="assetId">The asset ID</param>
        /// <param name="updates">Fields to update</param>
        public async Task<Asset> UpdateAssetAsync(
            string assetId,
            object updates,
            CancellationToken cancellationToken = default)
        {
            await EnsureAuthenticatedAsync(cancellationToken);
            var content = new StringContent(
                JsonSerializer.Serialize(updates),
                Encoding.UTF8,
                "application/json");
            var response = await _httpClient.PatchAsync($"/api/assets/{assetId}/", content, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Asset>(cancellationToken)
                ?? throw new InvalidOperationException("Failed to update asset");
        }

        /// <summary>
        /// Deletes an asset
        /// </summary>
        /// <param name="assetId">The asset ID</param>
        public async Task DeleteAssetAsync(string assetId, CancellationToken cancellationToken = default)
        {
            await EnsureAuthenticatedAsync(cancellationToken);
            var response = await _httpClient.DeleteAsync($"/api/assets/{assetId}/", cancellationToken);
            response.EnsureSuccessStatusCode();
        }

        #endregion

        #region Projects

        /// <summary>
        /// Lists all projects with optional filtering
        /// </summary>
        /// <param name="queryParams">Optional query parameters (e.g., asset_search)</param>
        public async Task<ProjectListResponse> ListProjectsAsync(
            Dictionary<string, string>? queryParams = null,
            CancellationToken cancellationToken = default)
        {
            await EnsureAuthenticatedAsync(cancellationToken);
            var url = BuildUrl("/api/projects/", queryParams);
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ProjectListResponse>(cancellationToken)
                ?? new ProjectListResponse();
        }

        /// <summary>
        /// Gets details for a specific project
        /// </summary>
        /// <param name="projectId">The project ID</param>
        public async Task<Project> GetProjectAsync(string projectId, CancellationToken cancellationToken = default)
        {
            await EnsureAuthenticatedAsync(cancellationToken);
            var response = await _httpClient.GetAsync($"/api/projects/{projectId}/", cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Project>(cancellationToken)
                ?? throw new InvalidOperationException("Project not found");
        }

        /// <summary>
        /// Creates a new project
        /// </summary>
        /// <param name="project">Project details</param>
        public async Task<Project> CreateProjectAsync(
            ProjectRequest project,
            CancellationToken cancellationToken = default)
        {
            await EnsureAuthenticatedAsync(cancellationToken);
            var response = await _httpClient.PostAsJsonAsync("/api/projects/", project, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Project>(cancellationToken)
                ?? throw new InvalidOperationException("Failed to create project");
        }

        /// <summary>
        /// Updates a project (partial update using PATCH)
        /// </summary>
        /// <param name="projectId">The project ID</param>
        /// <param name="updates">Fields to update</param>
        public async Task<Project> UpdateProjectAsync(
            string projectId,
            object updates,
            CancellationToken cancellationToken = default)
        {
            await EnsureAuthenticatedAsync(cancellationToken);
            var content = new StringContent(
                JsonSerializer.Serialize(updates),
                Encoding.UTF8,
                "application/json");
            var response = await _httpClient.PatchAsync($"/api/projects/{projectId}/", content, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Project>(cancellationToken)
                ?? throw new InvalidOperationException("Failed to update project");
        }

        /// <summary>
        /// Deletes a project
        /// </summary>
        /// <param name="projectId">The project ID</param>
        public async Task DeleteProjectAsync(string projectId, CancellationToken cancellationToken = default)
        {
            await EnsureAuthenticatedAsync(cancellationToken);
            var response = await _httpClient.DeleteAsync($"/api/projects/{projectId}/", cancellationToken);
            response.EnsureSuccessStatusCode();
        }

        #endregion

        #region Inspection Media

        /// <summary>
        /// Lists all inspection media with optional filtering
        /// </summary>
        /// <param name="queryParams">Optional query parameters for filtering</param>
        public async Task<InspectionMediaListResponse> ListInspectionMediaAsync(
            Dictionary<string, string>? queryParams = null,
            CancellationToken cancellationToken = default)
        {
            await EnsureAuthenticatedAsync(cancellationToken);
            var url = BuildUrl("/api/inspection-media/", queryParams);
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<InspectionMediaListResponse>(cancellationToken)
                ?? new InspectionMediaListResponse();
        }

        /// <summary>
        /// Gets details for a specific inspection media (includes refreshed upload URL if expired)
        /// </summary>
        /// <param name="mediaId">The media ID</param>
        public async Task<InspectionMedia> GetInspectionMediaAsync(
            string mediaId,
            CancellationToken cancellationToken = default)
        {
            await EnsureAuthenticatedAsync(cancellationToken);
            var response = await _httpClient.GetAsync($"/api/inspection-media/{mediaId}/", cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<InspectionMedia>(cancellationToken)
                ?? throw new InvalidOperationException("Inspection media not found");
        }

        /// <summary>
        /// Creates a new inspection media object (Part 1 of upload process)
        /// </summary>
        /// <param name="media">Media details</param>
        /// <returns>Media object with upload URL (valid for 15 minutes)</returns>
        public async Task<InspectionMedia> CreateInspectionMediaAsync(
            InspectionMediaRequest media,
            CancellationToken cancellationToken = default)
        {
            await EnsureAuthenticatedAsync(cancellationToken);
            var response = await _httpClient.PostAsJsonAsync("/api/inspection-media/", media, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<InspectionMedia>(cancellationToken)
                ?? throw new InvalidOperationException("Failed to create inspection media");
        }

        /// <summary>
        /// Uploads file content to inspection media (Part 2 of upload process)
        /// </summary>
        /// <param name="uploadUrl">The upload URL from CreateInspectionMediaAsync</param>
        /// <param name="fileContent">File content as byte array</param>
        /// <param name="contentType">Content type (e.g., image/jpeg)</param>
        public async Task UploadInspectionMediaFileAsync(
            string uploadUrl,
            byte[] fileContent,
            string contentType = "application/octet-stream",
            CancellationToken cancellationToken = default)
        {
            using var content = new ByteArrayContent(fileContent);
            content.Headers.ContentType = new MediaTypeHeaderValue(contentType);

            var response = await _httpClient.PutAsync(uploadUrl, content, cancellationToken);
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Complete media upload workflow (creates media object and uploads file)
        /// </summary>
        /// <param name="media">Media metadata</param>
        /// <param name="fileContent">File content</param>
        /// <param name="contentType">Content type</param>
        /// <returns>The uploaded media object</returns>
        public async Task<InspectionMedia> UploadInspectionMediaAsync(
            InspectionMediaRequest media,
            byte[] fileContent,
            string contentType = "application/octet-stream",
            CancellationToken cancellationToken = default)
        {
            // Part 1: Create media object
            var mediaObject = await CreateInspectionMediaAsync(media, cancellationToken);

            if (string.IsNullOrEmpty(mediaObject.UploadUrl))
            {
                throw new InvalidOperationException("No upload URL received from API");
            }

            // Part 2: Upload file
            await UploadInspectionMediaFileAsync(
                mediaObject.UploadUrl,
                fileContent,
                contentType,
                cancellationToken);

            return mediaObject;
        }

        /// <summary>
        /// Updates inspection media metadata (partial update using PATCH)
        /// </summary>
        /// <param name="mediaId">The media ID</param>
        /// <param name="updates">Metadata fields to update</param>
        public async Task<InspectionMedia> UpdateInspectionMediaMetadataAsync(
            string mediaId,
            object updates,
            CancellationToken cancellationToken = default)
        {
            await EnsureAuthenticatedAsync(cancellationToken);
            var content = new StringContent(
                JsonSerializer.Serialize(updates),
                Encoding.UTF8,
                "application/json");
            var response = await _httpClient.PatchAsync(
                $"/api/inspection-media/{mediaId}/",
                content,
                cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<InspectionMedia>(cancellationToken)
                ?? throw new InvalidOperationException("Failed to update inspection media");
        }

        #endregion

        #region Checklists

        /// <summary>
        /// Lists all checklists with optional filtering
        /// </summary>
        /// <param name="queryParams">Optional query parameters for filtering</param>
        public async Task<ChecklistListResponse> ListChecklistsAsync(
            Dictionary<string, string>? queryParams = null,
            CancellationToken cancellationToken = default)
        {
            await EnsureAuthenticatedAsync(cancellationToken);
            var url = BuildUrl("/api/checklists/", queryParams);
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ChecklistListResponse>(cancellationToken)
                ?? new ChecklistListResponse();
        }

        /// <summary>
        /// Gets details for a specific checklist
        /// </summary>
        /// <param name="checklistId">The checklist ID</param>
        public async Task<Checklist> GetChecklistAsync(
            string checklistId,
            CancellationToken cancellationToken = default)
        {
            await EnsureAuthenticatedAsync(cancellationToken);
            var response = await _httpClient.GetAsync($"/api/checklists/{checklistId}/", cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Checklist>(cancellationToken)
                ?? throw new InvalidOperationException("Checklist not found");
        }

        /// <summary>
        /// Creates a new checklist
        /// </summary>
        /// <param name="checklist">Checklist details</param>
        public async Task<Checklist> CreateChecklistAsync(
            ChecklistRequest checklist,
            CancellationToken cancellationToken = default)
        {
            await EnsureAuthenticatedAsync(cancellationToken);
            var response = await _httpClient.PostAsJsonAsync("/api/checklists/", checklist, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Checklist>(cancellationToken)
                ?? throw new InvalidOperationException("Failed to create checklist");
        }

        /// <summary>
        /// Updates a checklist (partial update using PATCH)
        /// </summary>
        /// <param name="checklistId">The checklist ID</param>
        /// <param name="updates">Fields to update</param>
        public async Task<Checklist> UpdateChecklistAsync(
            string checklistId,
            object updates,
            CancellationToken cancellationToken = default)
        {
            await EnsureAuthenticatedAsync(cancellationToken);
            var content = new StringContent(
                JsonSerializer.Serialize(updates),
                Encoding.UTF8,
                "application/json");
            var response = await _httpClient.PatchAsync(
                $"/api/checklists/{checklistId}/",
                content,
                cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Checklist>(cancellationToken)
                ?? throw new InvalidOperationException("Failed to update checklist");
        }

        #endregion

        #region Defects

        /// <summary>
        /// Lists all defects with optional filtering
        /// </summary>
        /// <param name="queryParams">Optional query parameters for filtering</param>
        public async Task<DefectListResponse> ListDefectsAsync(
            Dictionary<string, string>? queryParams = null,
            CancellationToken cancellationToken = default)
        {
            await EnsureAuthenticatedAsync(cancellationToken);
            var url = BuildUrl("/api/defects/", queryParams);
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<DefectListResponse>(cancellationToken)
                ?? new DefectListResponse();
        }

        /// <summary>
        /// Gets details for a specific defect
        /// </summary>
        /// <param name="defectId">The defect ID</param>
        public async Task<Defect> GetDefectAsync(string defectId, CancellationToken cancellationToken = default)
        {
            await EnsureAuthenticatedAsync(cancellationToken);
            var response = await _httpClient.GetAsync($"/api/defects/{defectId}/", cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Defect>(cancellationToken)
                ?? throw new InvalidOperationException("Defect not found");
        }

        /// <summary>
        /// Creates a new defect
        /// </summary>
        /// <param name="defect">Defect details</param>
        public async Task<Defect> CreateDefectAsync(DefectRequest defect, CancellationToken cancellationToken = default)
        {
            await EnsureAuthenticatedAsync(cancellationToken);
            var response = await _httpClient.PostAsJsonAsync("/api/defects/", defect, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Defect>(cancellationToken)
                ?? throw new InvalidOperationException("Failed to create defect");
        }

        /// <summary>
        /// Updates a defect (partial update using PATCH)
        /// </summary>
        /// <param name="defectId">The defect ID</param>
        /// <param name="updates">Fields to update</param>
        public async Task<Defect> UpdateDefectAsync(
            string defectId,
            object updates,
            CancellationToken cancellationToken = default)
        {
            await EnsureAuthenticatedAsync(cancellationToken);
            var content = new StringContent(
                JsonSerializer.Serialize(updates),
                Encoding.UTF8,
                "application/json");
            var response = await _httpClient.PatchAsync($"/api/defects/{defectId}/", content, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Defect>(cancellationToken)
                ?? throw new InvalidOperationException("Failed to update defect");
        }

        #endregion

        #region Measurements

        /// <summary>
        /// Lists all measurements with optional filtering
        /// </summary>
        /// <param name="queryParams">Optional query parameters for filtering</param>
        public async Task<MeasurementListResponse> ListMeasurementsAsync(
            Dictionary<string, string>? queryParams = null,
            CancellationToken cancellationToken = default)
        {
            await EnsureAuthenticatedAsync(cancellationToken);
            var url = BuildUrl("/api/measurements/", queryParams);
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<MeasurementListResponse>(cancellationToken)
                ?? new MeasurementListResponse();
        }

        /// <summary>
        /// Gets details for a specific measurement
        /// </summary>
        /// <param name="measurementId">The measurement ID</param>
        public async Task<Measurement> GetMeasurementAsync(
            string measurementId,
            CancellationToken cancellationToken = default)
        {
            await EnsureAuthenticatedAsync(cancellationToken);
            var response = await _httpClient.GetAsync($"/api/measurements/{measurementId}/", cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Measurement>(cancellationToken)
                ?? throw new InvalidOperationException("Measurement not found");
        }

        /// <summary>
        /// Creates a new measurement
        /// </summary>
        /// <param name="measurement">Measurement details</param>
        public async Task<Measurement> CreateMeasurementAsync(
            MeasurementRequest measurement,
            CancellationToken cancellationToken = default)
        {
            await EnsureAuthenticatedAsync(cancellationToken);
            var response = await _httpClient.PostAsJsonAsync("/api/measurements/", measurement, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Measurement>(cancellationToken)
                ?? throw new InvalidOperationException("Failed to create measurement");
        }

        /// <summary>
        /// Updates a measurement (partial update using PATCH)
        /// </summary>
        /// <param name="measurementId">The measurement ID</param>
        /// <param name="updates">Fields to update</param>
        public async Task<Measurement> UpdateMeasurementAsync(
            string measurementId,
            object updates,
            CancellationToken cancellationToken = default)
        {
            await EnsureAuthenticatedAsync(cancellationToken);
            var content = new StringContent(
                JsonSerializer.Serialize(updates),
                Encoding.UTF8,
                "application/json");
            var response = await _httpClient.PatchAsync(
                $"/api/measurements/{measurementId}/",
                content,
                cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Measurement>(cancellationToken)
                ?? throw new InvalidOperationException("Failed to update measurement");
        }

        #endregion

        #region Users

        /// <summary>
        /// Lists all users with optional filtering
        /// </summary>
        /// <param name="queryParams">Optional query parameters for filtering</param>
        public async Task<UserListResponse> ListUsersAsync(
            Dictionary<string, string>? queryParams = null,
            CancellationToken cancellationToken = default)
        {
            await EnsureAuthenticatedAsync(cancellationToken);
            var url = BuildUrl("/api/users/", queryParams);
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<UserListResponse>(cancellationToken)
                ?? new UserListResponse();
        }

        /// <summary>
        /// Gets details for a specific user
        /// </summary>
        /// <param name="userId">The user ID</param>
        public async Task<User> GetUserAsync(string userId, CancellationToken cancellationToken = default)
        {
            await EnsureAuthenticatedAsync(cancellationToken);
            var response = await _httpClient.GetAsync($"/api/users/{userId}/", cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<User>(cancellationToken)
                ?? throw new InvalidOperationException("User not found");
        }

        #endregion

        #region Workspaces

        /// <summary>
        /// Lists all workspaces with optional filtering
        /// </summary>
        /// <param name="queryParams">Optional query parameters for filtering</param>
        public async Task<WorkspaceListResponse> ListWorkspacesAsync(
            Dictionary<string, string>? queryParams = null,
            CancellationToken cancellationToken = default)
        {
            await EnsureAuthenticatedAsync(cancellationToken);
            var url = BuildUrl("/api/workspaces/", queryParams);
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<WorkspaceListResponse>(cancellationToken)
                ?? new WorkspaceListResponse();
        }

        /// <summary>
        /// Gets details for a specific workspace
        /// </summary>
        /// <param name="workspaceId">The workspace ID</param>
        public async Task<Workspace> GetWorkspaceAsync(
            string workspaceId,
            CancellationToken cancellationToken = default)
        {
            await EnsureAuthenticatedAsync(cancellationToken);
            var response = await _httpClient.GetAsync($"/api/workspaces/{workspaceId}/", cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Workspace>(cancellationToken)
                ?? throw new InvalidOperationException("Workspace not found");
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Builds a URL with query parameters
        /// </summary>
        private static string BuildUrl(string basePath, Dictionary<string, string>? queryParams)
        {
            if (queryParams == null || queryParams.Count == 0)
                return basePath;

            var queryString = string.Join("&",
                queryParams.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));

            return $"{basePath}?{queryString}";
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            _httpClient?.Dispose();
            _tokenLock?.Dispose();
        }

        #endregion
    }
}
