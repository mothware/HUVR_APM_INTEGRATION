using GeApmClient;
using GeApmClient.Models;

namespace GeApmClient.Examples
{
    /// <summary>
    /// Example usage of the GE APM On-Premises Client
    /// </summary>
    public class UsageExamples
    {
        /// <summary>
        /// Example 1: Basic login and authentication
        /// </summary>
        public static async Task LoginExample()
        {
            using var client = new GeApmClient("https://apm.company.com");

            // Login with credentials
            var loginResponse = await client.LoginAsync("username", "password");

            Console.WriteLine($"Login successful!");
            Console.WriteLine($"Token: {loginResponse.Token}");
            Console.WriteLine($"User: {loginResponse.UserName}");
            Console.WriteLine($"Expires: {loginResponse.ExpiresAt}");
        }

        /// <summary>
        /// Example 2: Query TMLs with filters
        /// </summary>
        public static async Task QueryTmlsExample()
        {
            using var client = new GeApmClient("https://apm.company.com");
            await client.LoginAsync("username", "password");

            // Get all active TMLs
            var result = await client.GetTmlsAsync(new QueryParameters
            {
                Filter = "Status eq 'Active'",
                Select = "EntityKey,TML_ID,Description,Nominal_Thickness,Status",
                OrderBy = "TML_ID asc",
                Top = 100
            });

            Console.WriteLine($"Found {result.Items.Count} active TMLs");
            foreach (var tml in result.Items)
            {
                Console.WriteLine($"  {tml.TmlId}: {tml.Description} - {tml.NominalThickness}mm");
            }
        }

        /// <summary>
        /// Example 3: Get TML by ID
        /// </summary>
        public static async Task GetTmlByIdExample()
        {
            using var client = new GeApmClient("https://apm.company.com");
            await client.LoginAsync("username", "password");

            // Get specific TML
            var tml = await client.GetTmlByIdAsync("TML-001");

            if (tml != null)
            {
                Console.WriteLine($"TML: {tml.TmlId}");
                Console.WriteLine($"Description: {tml.Description}");
                Console.WriteLine($"Equipment: {tml.EquipmentId}");
                Console.WriteLine($"Nominal Thickness: {tml.NominalThickness}mm");
                Console.WriteLine($"Minimum Thickness: {tml.MinimumThickness}mm");
                Console.WriteLine($"Last Inspection: {tml.LastInspectionDate}");
            }
        }

        /// <summary>
        /// Example 4: Get all measurements for a TML
        /// </summary>
        public static async Task GetTmlMeasurementsExample()
        {
            using var client = new GeApmClient("https://apm.company.com");
            await client.LoginAsync("username", "password");

            // Get all measurements for TML-001, ordered by date
            var measurements = await client.GetTmlMeasurementsAsync(
                "TML-001",
                new QueryParameters
                {
                    OrderBy = "Measurement_Date desc",
                    Top = 10
                });

            Console.WriteLine($"Latest 10 measurements for TML-001:");
            foreach (var measurement in measurements.Items)
            {
                Console.WriteLine($"  {measurement.MeasurementDate:yyyy-MM-dd}: " +
                    $"{measurement.MeasuredThickness}mm by {measurement.MeasurementTakenBy}");
            }
        }

        /// <summary>
        /// Example 5: Load a single TML measurement
        /// </summary>
        public static async Task LoadSingleMeasurementExample()
        {
            using var client = new GeApmClient("https://apm.company.com");
            await client.LoginAsync("username", "password");

            // Create new measurement
            var measurementRequest = new TmlMeasurementRequest
            {
                TmlId = "TML-001",
                MeasurementDate = DateTime.UtcNow,
                MeasuredThickness = 12.5m,
                MeasurementTakenBy = "John Doe",
                InspectionId = "INS-2025-001",
                ReadingNumber = 1,
                Temperature = 22.5m,
                Comments = "Normal reading, no issues detected",
                Status = "Active"
            };

            var result = await client.LoadTmlMeasurementAsync(measurementRequest);

            Console.WriteLine($"Measurement created successfully!");
            Console.WriteLine($"Entity Key: {result.EntityKey}");
            Console.WriteLine($"TML: {result.TmlId}");
            Console.WriteLine($"Thickness: {result.MeasuredThickness}mm");
        }

        /// <summary>
        /// Example 6: Load multiple TML measurements in batch
        /// </summary>
        public static async Task LoadBatchMeasurementsExample()
        {
            using var client = new GeApmClient("https://apm.company.com");
            await client.LoginAsync("username", "password");

            var measurements = new List<TmlMeasurementRequest>
            {
                new TmlMeasurementRequest
                {
                    TmlId = "TML-001",
                    MeasurementDate = DateTime.UtcNow,
                    MeasuredThickness = 12.5m,
                    MeasurementTakenBy = "John Doe",
                    Status = "Active"
                },
                new TmlMeasurementRequest
                {
                    TmlId = "TML-002",
                    MeasurementDate = DateTime.UtcNow,
                    MeasuredThickness = 15.2m,
                    MeasurementTakenBy = "Jane Smith",
                    Status = "Active"
                },
                new TmlMeasurementRequest
                {
                    TmlId = "TML-003",
                    MeasurementDate = DateTime.UtcNow,
                    MeasuredThickness = 10.8m,
                    MeasurementTakenBy = "Bob Johnson",
                    Status = "Active"
                }
            };

            var results = await client.LoadTmlMeasurementsBatchAsync(measurements);

            var successful = results.Count(r => r.Error == null);
            var failed = results.Count(r => r.Error != null);

            Console.WriteLine($"Batch complete: {successful} successful, {failed} failed");

            foreach (var (request, result, error) in results)
            {
                if (error == null)
                {
                    Console.WriteLine($"  ✓ {request.TmlId}: Created (Key: {result!.EntityKey})");
                }
                else
                {
                    Console.WriteLine($"  ✗ {request.TmlId}: Failed - {error.Message}");
                }
            }
        }

        /// <summary>
        /// Example 7: Get measurements for multiple TMLs
        /// </summary>
        public static async Task GetMultipleTmlMeasurementsExample()
        {
            using var client = new GeApmClient("https://apm.company.com");
            await client.LoginAsync("username", "password");

            var tmlIds = new[] { "TML-001", "TML-002", "TML-003", "TML-004", "TML-005" };

            var measurementsByTml = await client.GetMultipleTmlMeasurementsAsync(
                tmlIds,
                new QueryParameters
                {
                    OrderBy = "Measurement_Date desc",
                    Top = 1000
                });

            Console.WriteLine($"Retrieved measurements for {measurementsByTml.Count} TMLs:");

            foreach (var (tmlId, measurements) in measurementsByTml)
            {
                Console.WriteLine($"  {tmlId}: {measurements.Count} measurements");

                if (measurements.Count > 0)
                {
                    var latest = measurements.OrderByDescending(m => m.MeasurementDate).First();
                    Console.WriteLine($"    Latest: {latest.MeasurementDate:yyyy-MM-dd} - {latest.MeasuredThickness}mm");
                }
            }
        }

        /// <summary>
        /// Example 8: Generic query with path and parameters
        /// </summary>
        public static async Task GenericQueryExample()
        {
            using var client = new GeApmClient("https://apm.company.com");
            await client.LoginAsync("username", "password");

            // Query Equipment family
            var equipment = await client.QueryAsync<dynamic>(
                "Equipment",
                new QueryParameters
                {
                    Filter = "Status eq 'Active'",
                    Select = "EntityKey,EntityId,Name,Status,Location",
                    Top = 50
                });

            Console.WriteLine($"Found {equipment.Items.Count} active equipment items");

            // Query Work History
            var workHistory = await client.QueryAsync<dynamic>(
                "Work_History",
                new QueryParameters
                {
                    Filter = "Status eq 'Open'",
                    OrderBy = "ScheduledDate asc",
                    Top = 20
                });

            Console.WriteLine($"Found {workHistory.Items.Count} open work orders");
        }

        /// <summary>
        /// Example 9: Raw query with custom parameters
        /// </summary>
        public static async Task RawQueryExample()
        {
            using var client = new GeApmClient("https://apm.company.com");
            await client.LoginAsync("username", "password");

            // Custom parameters as dictionary
            var parameters = new Dictionary<string, string>
            {
                { "$filter", "Corrosion_Rate gt 0.5" },
                { "$select", "TML_ID,Description,Corrosion_Rate,Next_Inspection_Date" },
                { "$orderby", "Corrosion_Rate desc" },
                { "$top", "25" }
            };

            var jsonResponse = await client.QueryRawAsync(
                "Thickness_Measurement_Location",
                parameters);

            Console.WriteLine("Raw JSON Response:");
            Console.WriteLine(jsonResponse);

            // Parse and process as needed
            // var data = JsonSerializer.Deserialize<ODataResponse<dynamic>>(jsonResponse);
        }

        /// <summary>
        /// Example 10: Get entity count
        /// </summary>
        public static async Task GetCountExample()
        {
            using var client = new GeApmClient("https://apm.company.com");
            await client.LoginAsync("username", "password");

            // Total count of TMLs
            var totalTmls = await client.GetCountAsync("Thickness_Measurement_Location");
            Console.WriteLine($"Total TMLs: {totalTmls}");

            // Count with filter
            var activeTmls = await client.GetCountAsync(
                "Thickness_Measurement_Location",
                "Status eq 'Active'");
            Console.WriteLine($"Active TMLs: {activeTmls}");

            // Count of measurements for specific TML
            var measurementCount = await client.GetCountAsync(
                "TML_Measurement",
                "TML_ID eq 'TML-001'");
            Console.WriteLine($"Measurements for TML-001: {measurementCount}");
        }

        /// <summary>
        /// Example 11: Update existing measurement
        /// </summary>
        public static async Task UpdateMeasurementExample()
        {
            using var client = new GeApmClient("https://apm.company.com");
            await client.LoginAsync("username", "password");

            // Get measurement to update
            var measurements = await client.GetTmlMeasurementsAsync(
                "TML-001",
                new QueryParameters { Top = 1, OrderBy = "Measurement_Date desc" });

            if (measurements.Items.Count > 0)
            {
                var measurement = measurements.Items.First();

                // Update specific fields
                var updates = new Dictionary<string, object?>
                {
                    { "Comments", "Updated: Reviewed and verified measurement" },
                    { "Temperature", 23.5m },
                    { "Modified_Date", DateTime.UtcNow }
                };

                var updated = await client.UpdateTmlMeasurementAsync(
                    measurement.EntityKey,
                    updates);

                Console.WriteLine($"Measurement {updated.EntityKey} updated successfully");
                Console.WriteLine($"New comments: {updated.Comments}");
            }
        }

        /// <summary>
        /// Example 12: Pagination through large result sets
        /// </summary>
        public static async Task PaginationExample()
        {
            using var client = new GeApmClient("https://apm.company.com");
            await client.LoginAsync("username", "password");

            int pageSize = 100;
            int page = 0;
            int totalProcessed = 0;

            while (true)
            {
                var result = await client.GetTmlsAsync(new QueryParameters
                {
                    Filter = "Status eq 'Active'",
                    OrderBy = "TML_ID asc",
                    Top = pageSize,
                    Skip = page * pageSize,
                    Count = true
                });

                Console.WriteLine($"Page {page + 1}: Retrieved {result.Items.Count} TMLs");

                // Process items
                foreach (var tml in result.Items)
                {
                    // Do something with each TML
                    totalProcessed++;
                }

                // Check if we have more pages
                if (result.Items.Count < pageSize || !result.HasMore)
                {
                    break;
                }

                page++;
            }

            Console.WriteLine($"Total TMLs processed: {totalProcessed}");
        }

        /// <summary>
        /// Example 13: Complex filtering
        /// </summary>
        public static async Task ComplexFilterExample()
        {
            using var client = new GeApmClient("https://apm.company.com");
            await client.LoginAsync("username", "password");

            // Find TMLs that need attention (high corrosion rate or below minimum thickness)
            var criticalTmls = await client.QueryAsync<ThicknessMeasurementLocation>(
                "Thickness_Measurement_Location",
                new QueryParameters
                {
                    Filter = "(Corrosion_Rate gt 1.0) or " +
                             "(Nominal_Thickness sub Minimum_Thickness lt 2.0)",
                    Select = "TML_ID,Description,Corrosion_Rate,Nominal_Thickness,Minimum_Thickness",
                    OrderBy = "Corrosion_Rate desc"
                });

            Console.WriteLine($"Found {criticalTmls.Items.Count} critical TMLs:");
            foreach (var tml in criticalTmls.Items)
            {
                Console.WriteLine($"  {tml.TmlId}: CR={tml.CorrosionRate}, " +
                    $"Nominal={tml.NominalThickness}, Min={tml.MinimumThickness}");
            }
        }

        /// <summary>
        /// Example 14: Working with Equipment and TMLs
        /// </summary>
        public static async Task EquipmentTmlsExample()
        {
            using var client = new GeApmClient("https://apm.company.com");
            await client.LoginAsync("username", "password");

            // Get TMLs for specific equipment
            var equipmentId = "VESSEL-001";

            var tmls = await client.GetTmlsAsync(new QueryParameters
            {
                Filter = $"Equipment_ID eq '{equipmentId}'",
                OrderBy = "Location asc"
            });

            Console.WriteLine($"TMLs for {equipmentId}:");
            foreach (var tml in tmls.Items)
            {
                Console.WriteLine($"  {tml.TmlId} at {tml.Location}");

                // Get latest measurement for each TML
                var measurements = await client.GetTmlMeasurementsAsync(
                    tml.TmlId!,
                    new QueryParameters
                    {
                        Top = 1,
                        OrderBy = "Measurement_Date desc"
                    });

                if (measurements.Items.Count > 0)
                {
                    var latest = measurements.Items.First();
                    Console.WriteLine($"    Latest: {latest.MeasurementDate:yyyy-MM-dd} - " +
                        $"{latest.MeasuredThickness}mm");
                }
            }
        }

        /// <summary>
        /// Example 15: Error handling
        /// </summary>
        public static async Task ErrorHandlingExample()
        {
            using var client = new GeApmClient("https://apm.company.com");

            try
            {
                // Attempt login
                await client.LoginAsync("username", "password");
                Console.WriteLine("Login successful");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Login failed: {ex.Message}");
                return;
            }

            try
            {
                // Try to get non-existent TML
                var tml = await client.GetTmlByKeyAsync(999999);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"TML not found: {ex.Message}");
            }

            try
            {
                // Load measurement with validation
                var measurement = new TmlMeasurementRequest
                {
                    TmlId = "TML-001",
                    MeasurementDate = DateTime.UtcNow,
                    MeasuredThickness = 12.5m,
                    Status = "Active"
                };

                var result = await client.LoadTmlMeasurementAsync(measurement);
                Console.WriteLine($"Measurement loaded: {result.EntityKey}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load measurement: {ex.Message}");
            }
        }
    }
}
