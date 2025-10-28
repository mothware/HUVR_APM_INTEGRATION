using HuvrApiClient.Models;

namespace HuvrApiClient
{
    /// <summary>
    /// Utility class for gathering and exporting data from HUVR API
    /// </summary>
    public class HuvrDataGatherer
    {
        private readonly HuvrApiClient _client;

        public HuvrDataGatherer(HuvrApiClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        /// <summary>
        /// Represents a complete project data snapshot with all related entities
        /// </summary>
        public class ProjectDataSnapshot
        {
            public Project Project { get; set; } = null!;
            public Asset? Asset { get; set; }
            public List<InspectionMedia> Media { get; set; } = new();
            public List<Checklist> Checklists { get; set; } = new();
            public List<Defect> Defects { get; set; } = new();
            public List<Measurement> Measurements { get; set; } = new();
        }

        /// <summary>
        /// Gathers all data related to a specific project
        /// </summary>
        /// <param name="projectId">The project ID to gather data for</param>
        /// <param name="includeAsset">Whether to include asset details (default: true)</param>
        public async Task<ProjectDataSnapshot> GatherProjectDataAsync(
            string projectId,
            bool includeAsset = true,
            CancellationToken cancellationToken = default)
        {
            var snapshot = new ProjectDataSnapshot();

            // Get project details
            snapshot.Project = await _client.GetProjectAsync(projectId, cancellationToken);

            // Get asset if requested
            if (includeAsset && !string.IsNullOrEmpty(snapshot.Project.AssetId))
            {
                try
                {
                    snapshot.Asset = await _client.GetAssetAsync(snapshot.Project.AssetId, cancellationToken);
                }
                catch (HttpRequestException)
                {
                    // Asset might not exist or be accessible
                    snapshot.Asset = null;
                }
            }

            // Get all media for this project
            var mediaResponse = await _client.ListInspectionMediaAsync(
                new Dictionary<string, string> { { "project_id", projectId } },
                cancellationToken);
            snapshot.Media = mediaResponse.Results;

            // Get all checklists for this project
            var checklistResponse = await _client.ListChecklistsAsync(
                new Dictionary<string, string> { { "project_id", projectId } },
                cancellationToken);
            snapshot.Checklists = checklistResponse.Results;

            // Get all defects for this project
            var defectResponse = await _client.ListDefectsAsync(
                new Dictionary<string, string> { { "project_id", projectId } },
                cancellationToken);
            snapshot.Defects = defectResponse.Results;

            // Get all measurements for this project
            var measurementResponse = await _client.ListMeasurementsAsync(
                new Dictionary<string, string> { { "project_id", projectId } },
                cancellationToken);
            snapshot.Measurements = measurementResponse.Results;

            return snapshot;
        }

        /// <summary>
        /// Gathers data for multiple projects in parallel
        /// </summary>
        /// <param name="projectIds">List of project IDs</param>
        /// <param name="includeAsset">Whether to include asset details</param>
        /// <param name="maxConcurrency">Maximum number of concurrent requests (default: 5)</param>
        public async Task<List<ProjectDataSnapshot>> GatherMultipleProjectsDataAsync(
            List<string> projectIds,
            bool includeAsset = true,
            int maxConcurrency = 5,
            CancellationToken cancellationToken = default)
        {
            var semaphore = new SemaphoreSlim(maxConcurrency);
            var tasks = new List<Task<ProjectDataSnapshot>>();

            foreach (var projectId in projectIds)
            {
                await semaphore.WaitAsync(cancellationToken);

                var task = Task.Run(async () =>
                {
                    try
                    {
                        return await GatherProjectDataAsync(projectId, includeAsset, cancellationToken);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }, cancellationToken);

                tasks.Add(task);
            }

            return (await Task.WhenAll(tasks)).ToList();
        }

        /// <summary>
        /// Gathers all data for projects matching the given criteria
        /// </summary>
        /// <param name="queryParams">Query parameters to filter projects</param>
        /// <param name="includeAsset">Whether to include asset details</param>
        /// <param name="maxProjects">Maximum number of projects to process</param>
        public async Task<List<ProjectDataSnapshot>> GatherProjectsDataByFilterAsync(
            Dictionary<string, string>? queryParams = null,
            bool includeAsset = true,
            int? maxProjects = null,
            CancellationToken cancellationToken = default)
        {
            // Get all projects matching the filter
            var projects = await _client.GetAllProjectsAsync(queryParams, maxProjects, cancellationToken);

            // Gather data for each project
            var projectIds = projects.Select(p => p.Id!).ToList();
            return await GatherMultipleProjectsDataAsync(projectIds, includeAsset, 5, cancellationToken);
        }

        /// <summary>
        /// Gathers all data for a specific asset including all its projects
        /// </summary>
        /// <param name="assetId">The asset ID</param>
        public async Task<AssetDataSnapshot> GatherAssetDataAsync(
            string assetId,
            CancellationToken cancellationToken = default)
        {
            var snapshot = new AssetDataSnapshot
            {
                Asset = await _client.GetAssetAsync(assetId, cancellationToken)
            };

            // Get all projects for this asset
            var projectsResponse = await _client.ListProjectsAsync(
                new Dictionary<string, string> { { "asset_search", assetId } },
                cancellationToken);

            snapshot.Projects = projectsResponse.Results;

            // Gather full data for each project
            var projectIds = snapshot.Projects.Select(p => p.Id!).ToList();
            snapshot.ProjectSnapshots = await GatherMultipleProjectsDataAsync(
                projectIds,
                includeAsset: false, // We already have the asset
                cancellationToken: cancellationToken);

            return snapshot;
        }

        /// <summary>
        /// Represents complete asset data including all projects and related data
        /// </summary>
        public class AssetDataSnapshot
        {
            public Asset Asset { get; set; } = null!;
            public List<Project> Projects { get; set; } = new();
            public List<ProjectDataSnapshot> ProjectSnapshots { get; set; } = new();
        }

        /// <summary>
        /// Exports defects summary data
        /// </summary>
        /// <param name="queryParams">Optional filters for defects</param>
        public async Task<DefectsSummary> GetDefectsSummaryAsync(
            Dictionary<string, string>? queryParams = null,
            CancellationToken cancellationToken = default)
        {
            var defects = await _client.GetAllDefectsAsync(queryParams, cancellationToken: cancellationToken);

            var summary = new DefectsSummary
            {
                TotalDefects = defects.Count,
                BySeverity = defects.GroupBy(d => d.Severity ?? "Unknown")
                    .ToDictionary(g => g.Key, g => g.Count()),
                ByStatus = defects.GroupBy(d => d.Status ?? "Unknown")
                    .ToDictionary(g => g.Key, g => g.Count()),
                ByType = defects.GroupBy(d => d.DefectType ?? "Unknown")
                    .ToDictionary(g => g.Key, g => g.Count()),
                Defects = defects
            };

            return summary;
        }

        /// <summary>
        /// Summary statistics for defects
        /// </summary>
        public class DefectsSummary
        {
            public int TotalDefects { get; set; }
            public Dictionary<string, int> BySeverity { get; set; } = new();
            public Dictionary<string, int> ByStatus { get; set; } = new();
            public Dictionary<string, int> ByType { get; set; } = new();
            public List<Defect> Defects { get; set; } = new();
        }

        /// <summary>
        /// Downloads media file content
        /// </summary>
        /// <param name="media">The media object with download URL</param>
        public async Task<byte[]> DownloadMediaFileAsync(
            InspectionMedia media,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(media.DownloadUrl))
            {
                throw new InvalidOperationException("Media does not have a download URL");
            }

            using var httpClient = new HttpClient();
            return await httpClient.GetByteArrayAsync(media.DownloadUrl, cancellationToken);
        }

        /// <summary>
        /// Downloads all media files for a project
        /// </summary>
        /// <param name="projectId">The project ID</param>
        /// <param name="outputDirectory">Directory to save files</param>
        public async Task<List<string>> DownloadProjectMediaAsync(
            string projectId,
            string outputDirectory,
            CancellationToken cancellationToken = default)
        {
            var mediaResponse = await _client.ListInspectionMediaAsync(
                new Dictionary<string, string> { { "project_id", projectId } },
                cancellationToken);

            var downloadedFiles = new List<string>();

            Directory.CreateDirectory(outputDirectory);

            foreach (var media in mediaResponse.Results)
            {
                if (!string.IsNullOrEmpty(media.DownloadUrl))
                {
                    try
                    {
                        var fileBytes = await DownloadMediaFileAsync(media, cancellationToken);
                        var filePath = Path.Combine(outputDirectory, media.FileName);

                        await File.WriteAllBytesAsync(filePath, fileBytes, cancellationToken);
                        downloadedFiles.Add(filePath);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to download {media.FileName}: {ex.Message}");
                    }
                }
            }

            return downloadedFiles;
        }

        /// <summary>
        /// Gets workspace summary with user and project counts
        /// </summary>
        public async Task<WorkspaceSummary> GetWorkspaceSummaryAsync(
            CancellationToken cancellationToken = default)
        {
            var workspaces = await _client.ListWorkspacesAsync(cancellationToken: cancellationToken);
            var users = await _client.ListUsersAsync(cancellationToken: cancellationToken);
            var projects = await _client.ListProjectsAsync(cancellationToken: cancellationToken);
            var assets = await _client.ListAssetsAsync(cancellationToken: cancellationToken);

            return new WorkspaceSummary
            {
                Workspaces = workspaces.Results,
                TotalUsers = users.Count,
                TotalProjects = projects.Count,
                TotalAssets = assets.Count,
                ActiveUsers = users.Results.Count(u => u.IsActive)
            };
        }

        /// <summary>
        /// Workspace summary information
        /// </summary>
        public class WorkspaceSummary
        {
            public List<Workspace> Workspaces { get; set; } = new();
            public int TotalUsers { get; set; }
            public int TotalProjects { get; set; }
            public int TotalAssets { get; set; }
            public int ActiveUsers { get; set; }
        }
    }
}
