using HuvrApiClient;
using HuvrApiClient.Models;

namespace HuvrApiClient.Examples
{
    /// <summary>
    /// Example usage of the HUVR API Client
    /// </summary>
    public class UsageExamples
    {
        /// <summary>
        /// Example 1: Basic setup and authentication
        /// </summary>
        public static async System.Threading.Tasks.Task BasicSetupExample()
        {
            // Initialize the client with your credentials
            var client = new HuvrApiClient(
                clientId: "[email protected]",
                clientSecret: "your-secret-here"
            );

            // Obtain access token (this is done automatically, but you can call it explicitly)
            var token = await client.ObtainAccessTokenAsync();
            Console.WriteLine($"Access token obtained, expires at: {token.ExpiresAt}");
        }

        /// <summary>
        /// Example 2: Asset operations
        /// </summary>
        public static async System.Threading.Tasks.Task AssetOperationsExample()
        {
            using var client = new HuvrApiClient("your-client-id", "your-client-secret");

            // List all assets
            var assets = await client.ListAssetsAsync();
            Console.WriteLine($"Found {assets.Count} assets");

            // Create a new asset
            var newAsset = await client.CreateAssetAsync(new AssetRequest
            {
                Name = "Pump #123",
                Description = "Main cooling pump",
                AssetType = "Pump",
                Location = "Building A, Floor 2",
                Status = "Active"
            });
            Console.WriteLine($"Created asset with ID: {newAsset.Id}");

            // Get asset details
            var asset = await client.GetAssetAsync(newAsset.Id!);
            Console.WriteLine($"Asset name: {asset.Name}");

            // Update asset (partial update)
            var updatedAsset = await client.UpdateAssetAsync(
                newAsset.Id!,
                new { Status = "Maintenance" }
            );
            Console.WriteLine($"Updated asset status to: {updatedAsset.Status}");

            // Delete asset
            // await client.DeleteAssetAsync(newAsset.Id!);
        }

        /// <summary>
        /// Example 3: Project (work order) workflow
        /// </summary>
        public static async System.Threading.Tasks.Task ProjectWorkflowExample()
        {
            using var client = new HuvrApiClient("your-client-id", "your-client-secret");

            // List projects for a specific asset
            var projects = await client.ListProjectsAsync(new Dictionary<string, string>
            {
                { "asset_search", "pump-123" }
            });

            // Create a new inspection project
            var project = await client.CreateProjectAsync(new ProjectRequest
            {
                Name = "Quarterly Inspection Q1 2025",
                Description = "Routine quarterly inspection",
                AssetId = "asset-id-here",
                ProjectTypeId = "inspection-type-id",
                Status = "In Progress",
                StartDate = DateTime.UtcNow
            });
            Console.WriteLine($"Created project: {project.Id}");

            // Update project status
            await client.UpdateProjectAsync(project.Id!, new { Status = "Completed" });
        }

        /// <summary>
        /// Example 4: Media upload workflow
        /// </summary>
        public static async System.Threading.Tasks.Task MediaUploadExample()
        {
            using var client = new HuvrApiClient("your-client-id", "your-client-secret");

            // Read file content
            var filePath = "inspection-photo.jpg";
            var fileBytes = await File.ReadAllBytesAsync(filePath);

            // Complete upload workflow (creates media object and uploads file)
            var media = await client.UploadInspectionMediaAsync(
                new InspectionMediaRequest
                {
                    ProjectId = "project-id-here",
                    FileName = "inspection-photo.jpg",
                    FileType = "image/jpeg",
                    FileSize = fileBytes.Length,
                    Metadata = new Dictionary<string, object>
                    {
                        { "camera", "DJI Mavic 3" },
                        { "location", "North wall" }
                    }
                },
                fileBytes,
                "image/jpeg"
            );

            Console.WriteLine($"Uploaded media: {media.Id}");
            Console.WriteLine($"Thumbnail URL: {media.ThumbnailUrl}");

            // Update media metadata
            await client.UpdateInspectionMediaMetadataAsync(
                media.Id!,
                new { Metadata = new { Notes = "Crack detected in upper section" } }
            );
        }

        /// <summary>
        /// Example 5: Manual two-part media upload with retry logic
        /// </summary>
        public static async System.Threading.Tasks.Task ManualMediaUploadWithRetryExample()
        {
            using var client = new HuvrApiClient("your-client-id", "your-client-secret");

            var fileBytes = await File.ReadAllBytesAsync("inspection-photo.jpg");

            // Part 1: Create media object
            var media = await client.CreateInspectionMediaAsync(new InspectionMediaRequest
            {
                ProjectId = "project-id-here",
                FileName = "inspection-photo.jpg",
                FileType = "image/jpeg",
                FileSize = fileBytes.Length
            });

            // Part 2: Upload file with retry on expiration
            try
            {
                await client.UploadInspectionMediaFileAsync(
                    media.UploadUrl!,
                    fileBytes,
                    "image/jpeg"
                );
            }
            catch (HttpRequestException)
            {
                // URL expired, get fresh URL
                media = await client.GetInspectionMediaAsync(media.Id!);
                await client.UploadInspectionMediaFileAsync(
                    media.UploadUrl!,
                    fileBytes,
                    "image/jpeg"
                );
            }
        }

        /// <summary>
        /// Example 6: Checklist operations
        /// </summary>
        public static async System.Threading.Tasks.Task ChecklistExample()
        {
            using var client = new HuvrApiClient("your-client-id", "your-client-secret");

            // Create a checklist
            var checklist = await client.CreateChecklistAsync(new ChecklistRequest
            {
                ProjectId = "project-id-here",
                Name = "Safety Inspection Checklist",
                TemplateId = "template-id-here",
                Status = "In Progress",
                Responses = new Dictionary<string, object>
                {
                    { "question1", "Yes" },
                    { "question2", "No" },
                    { "notes", "Minor wear observed" }
                }
            });

            // Update checklist responses
            await client.UpdateChecklistAsync(
                checklist.Id!,
                new
                {
                    Status = "Completed",
                    Responses = new Dictionary<string, object>
                    {
                        { "question1", "Yes" },
                        { "question2", "Yes" },
                        { "question3", "Yes" }
                    }
                }
            );
        }

        /// <summary>
        /// Example 7: Defect (findings) management
        /// </summary>
        public static async System.Threading.Tasks.Task DefectExample()
        {
            using var client = new HuvrApiClient("your-client-id", "your-client-secret");

            // Create a defect
            var defect = await client.CreateDefectAsync(new DefectRequest
            {
                ProjectId = "project-id-here",
                AssetId = "asset-id-here",
                Title = "Crack in foundation",
                Description = "15cm crack detected in north wall foundation",
                Severity = "High",
                Status = "Open",
                DefectType = "Structural",
                Location = "North wall, base level",
                Metadata = new Dictionary<string, object>
                {
                    { "length_cm", 15 },
                    { "width_mm", 3 }
                }
            });

            Console.WriteLine($"Created defect: {defect.Id}");

            // Update defect status
            await client.UpdateDefectAsync(
                defect.Id!,
                new { Status = "In Repair", Severity = "Critical" }
            );

            // List all high-severity defects
            var defects = await client.ListDefectsAsync(new Dictionary<string, string>
            {
                { "severity", "High" }
            });
        }

        /// <summary>
        /// Example 8: Measurement recording
        /// </summary>
        public static async System.Threading.Tasks.Task MeasurementExample()
        {
            using var client = new HuvrApiClient("your-client-id", "your-client-secret");

            // Record ultrasonic testing measurement
            var measurement = await client.CreateMeasurementAsync(new MeasurementRequest
            {
                ProjectId = "project-id-here",
                AssetId = "asset-id-here",
                MeasurementType = "Ultrasonic Thickness",
                Value = 12.5m,
                Unit = "mm",
                Location = "Point A1",
                Metadata = new Dictionary<string, object>
                {
                    { "temperature_c", 22 },
                    { "humidity_percent", 45 },
                    { "equipment", "UT-1000" }
                }
            });

            Console.WriteLine($"Recorded measurement: {measurement.Value} {measurement.Unit}");
        }

        /// <summary>
        /// Example 9: Complete inspection workflow
        /// </summary>
        public static async System.Threading.Tasks.Task CompleteInspectionWorkflowExample()
        {
            using var client = new HuvrApiClient("your-client-id", "your-client-secret");

            // Step 1: Get the asset to inspect
            var assets = await client.ListAssetsAsync(new Dictionary<string, string>
            {
                { "name", "Pump #123" }
            });
            var asset = assets.Results.FirstOrDefault();

            if (asset == null)
            {
                Console.WriteLine("Asset not found");
                return;
            }

            // Step 2: Create inspection project
            var project = await client.CreateProjectAsync(new ProjectRequest
            {
                Name = $"Monthly Inspection - {DateTime.Now:yyyy-MM}",
                AssetId = asset.Id!,
                ProjectTypeId = "inspection-type-id",
                Status = "In Progress",
                StartDate = DateTime.UtcNow
            });

            // Step 3: Upload inspection photos
            var photoFiles = Directory.GetFiles("./inspection-photos", "*.jpg");
            foreach (var photoFile in photoFiles)
            {
                var fileBytes = await File.ReadAllBytesAsync(photoFile);
                await client.UploadInspectionMediaAsync(
                    new InspectionMediaRequest
                    {
                        ProjectId = project.Id!,
                        FileName = Path.GetFileName(photoFile),
                        FileType = "image/jpeg",
                        FileSize = fileBytes.Length
                    },
                    fileBytes,
                    "image/jpeg"
                );
            }

            // Step 4: Complete checklist
            var checklist = await client.CreateChecklistAsync(new ChecklistRequest
            {
                ProjectId = project.Id!,
                Name = "Standard Inspection Checklist",
                Status = "Completed",
                Responses = new Dictionary<string, object>
                {
                    { "visual_inspection", "Pass" },
                    { "leak_check", "Pass" },
                    { "noise_level", "Normal" }
                }
            });

            // Step 5: Record any defects
            var defect = await client.CreateDefectAsync(new DefectRequest
            {
                ProjectId = project.Id!,
                AssetId = asset.Id!,
                Title = "Minor corrosion",
                Description = "Surface corrosion on mounting bracket",
                Severity = "Low",
                Status = "Open",
                DefectType = "Corrosion"
            });

            // Step 6: Record measurements
            await client.CreateMeasurementAsync(new MeasurementRequest
            {
                ProjectId = project.Id!,
                AssetId = asset.Id!,
                MeasurementType = "Vibration",
                Value = 2.5m,
                Unit = "mm/s"
            });

            // Step 7: Complete the project
            await client.UpdateProjectAsync(
                project.Id!,
                new { Status = "Completed", EndDate = DateTime.UtcNow }
            );

            Console.WriteLine($"Inspection completed for {asset.Name}");
        }

        /// <summary>
        /// Example 10: Error handling and token refresh
        /// </summary>
        public static async System.Threading.Tasks.Task ErrorHandlingExample()
        {
            using var client = new HuvrApiClient("your-client-id", "your-client-secret");

            try
            {
                // The client automatically handles token refresh
                // If a 401 is encountered, it will refresh the token and retry
                var assets = await client.ListAssetsAsync();

                // Handle specific operations
                foreach (var asset in assets.Results)
                {
                    try
                    {
                        var details = await client.GetAssetAsync(asset.Id!);
                        Console.WriteLine($"Processed: {details.Name}");
                    }
                    catch (HttpRequestException ex)
                    {
                        Console.WriteLine($"Failed to get asset {asset.Id}: {ex.Message}");
                        // Continue with next asset
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
