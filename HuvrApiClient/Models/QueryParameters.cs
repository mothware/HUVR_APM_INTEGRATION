using System.Globalization;

namespace HuvrApiClient.Models
{
    /// <summary>
    /// Base query parameters for pagination
    /// </summary>
    public class BaseQueryParameters
    {
        /// <summary>
        /// Number of results per page (default: 50)
        /// </summary>
        public int? Limit { get; set; }

        /// <summary>
        /// Pagination offset
        /// </summary>
        public int? Offset { get; set; }

        /// <summary>
        /// Converts query parameters to dictionary format
        /// </summary>
        public virtual Dictionary<string, string> ToDictionary()
        {
            var dict = new Dictionary<string, string>();

            if (Limit.HasValue)
                dict["limit"] = Limit.Value.ToString();

            if (Offset.HasValue)
                dict["offset"] = Offset.Value.ToString();

            return dict;
        }
    }

    /// <summary>
    /// Query parameters for filtering assets
    /// </summary>
    public class AssetQueryParameters : BaseQueryParameters
    {
        /// <summary>
        /// Filter by external ID (identifier from external system)
        /// </summary>
        public string? ExternalId { get; set; }

        /// <summary>
        /// Filter by asset name
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Filter by asset type
        /// </summary>
        public string? AssetType { get; set; }

        /// <summary>
        /// Filter by location
        /// </summary>
        public string? Location { get; set; }

        /// <summary>
        /// Filter by status (e.g., "Active", "Inactive", "Maintenance")
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// Include library information in the response
        /// </summary>
        public bool IncludeLibrary { get; set; } = false;

        public override Dictionary<string, string> ToDictionary()
        {
            var dict = base.ToDictionary();

            if (!string.IsNullOrEmpty(ExternalId))
                dict["external_id"] = ExternalId;

            if (!string.IsNullOrEmpty(Name))
                dict["name"] = Name;

            if (!string.IsNullOrEmpty(AssetType))
                dict["asset_type"] = AssetType;

            if (!string.IsNullOrEmpty(Location))
                dict["location"] = Location;

            if (!string.IsNullOrEmpty(Status))
                dict["status"] = Status;

            if (IncludeLibrary)
                dict["include"] = "library";

            return dict;
        }
    }

    /// <summary>
    /// Query parameters for filtering projects
    /// </summary>
    public class ProjectQueryParameters : BaseQueryParameters
    {
        /// <summary>
        /// Filter by asset ID or name
        /// </summary>
        public string? AssetSearch { get; set; }

        /// <summary>
        /// Filter by project status (e.g., "Scheduled", "In Progress", "Completed")
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// Filter by project type ID
        /// </summary>
        public string? ProjectTypeId { get; set; }

        /// <summary>
        /// Filter by start date (projects starting after this date)
        /// </summary>
        public DateTime? StartDateAfter { get; set; }

        /// <summary>
        /// Filter by end date (projects ending before this date)
        /// </summary>
        public DateTime? EndDateBefore { get; set; }

        /// <summary>
        /// Filter by project name
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Include parent asset information in the response
        /// </summary>
        public bool IncludeParent { get; set; } = false;

        /// <summary>
        /// Include library information in the response
        /// </summary>
        public bool IncludeLibrary { get; set; } = false;

        /// <summary>
        /// Include defects in the response
        /// </summary>
        public bool IncludeDefects { get; set; } = false;

        public override Dictionary<string, string> ToDictionary()
        {
            var dict = base.ToDictionary();

            if (!string.IsNullOrEmpty(AssetSearch))
                dict["asset_search"] = AssetSearch;

            if (!string.IsNullOrEmpty(Status))
                dict["status"] = Status;

            if (!string.IsNullOrEmpty(ProjectTypeId))
                dict["project_type_id"] = ProjectTypeId;

            if (StartDateAfter.HasValue)
                dict["start_date_after"] = StartDateAfter.Value.ToString("yyyy-MM-ddTHH:mm:ssZ");

            if (EndDateBefore.HasValue)
                dict["end_date_before"] = EndDateBefore.Value.ToString("yyyy-MM-ddTHH:mm:ssZ");

            if (!string.IsNullOrEmpty(Name))
                dict["name"] = Name;

            // Build include parameter
            var includes = new List<string>();
            if (IncludeParent)
                includes.Add("parent");
            if (IncludeLibrary)
                includes.Add("library");
            if (IncludeDefects)
                includes.Add("defects");

            if (includes.Any())
                dict["include"] = string.Join(",", includes);

            return dict;
        }
    }

    /// <summary>
    /// Query parameters for filtering inspection media
    /// </summary>
    public class InspectionMediaQueryParameters : BaseQueryParameters
    {
        /// <summary>
        /// Filter by project ID
        /// </summary>
        public string? ProjectId { get; set; }

        /// <summary>
        /// Filter by file type (e.g., "image/jpeg", "video/mp4")
        /// </summary>
        public string? FileType { get; set; }

        /// <summary>
        /// Filter by upload status (e.g., "PENDING", "UPLOADED", "FAILED")
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// Filter by file name
        /// </summary>
        public string? FileName { get; set; }

        public override Dictionary<string, string> ToDictionary()
        {
            var dict = base.ToDictionary();

            if (!string.IsNullOrEmpty(ProjectId))
                dict["project_id"] = ProjectId;

            if (!string.IsNullOrEmpty(FileType))
                dict["file_type"] = FileType;

            if (!string.IsNullOrEmpty(Status))
                dict["status"] = Status;

            if (!string.IsNullOrEmpty(FileName))
                dict["file_name"] = FileName;

            return dict;
        }
    }

    /// <summary>
    /// Query parameters for filtering defects
    /// </summary>
    public class DefectQueryParameters : BaseQueryParameters
    {
        /// <summary>
        /// Filter by project ID
        /// </summary>
        public string? ProjectId { get; set; }

        /// <summary>
        /// Filter by asset ID
        /// </summary>
        public string? AssetId { get; set; }

        /// <summary>
        /// Filter by severity (e.g., "Low", "Medium", "High", "Critical")
        /// </summary>
        public string? Severity { get; set; }

        /// <summary>
        /// Filter by status (e.g., "Open", "In Progress", "Resolved", "Closed")
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// Filter by defect type (e.g., "Structural", "Corrosion", "Leak")
        /// </summary>
        public string? DefectType { get; set; }

        /// <summary>
        /// Filter by location
        /// </summary>
        public string? Location { get; set; }

        public override Dictionary<string, string> ToDictionary()
        {
            var dict = base.ToDictionary();

            if (!string.IsNullOrEmpty(ProjectId))
                dict["project_id"] = ProjectId;

            if (!string.IsNullOrEmpty(AssetId))
                dict["asset_id"] = AssetId;

            if (!string.IsNullOrEmpty(Severity))
                dict["severity"] = Severity;

            if (!string.IsNullOrEmpty(Status))
                dict["status"] = Status;

            if (!string.IsNullOrEmpty(DefectType))
                dict["defect_type"] = DefectType;

            if (!string.IsNullOrEmpty(Location))
                dict["location"] = Location;

            return dict;
        }
    }

    /// <summary>
    /// Query parameters for filtering measurements
    /// </summary>
    public class MeasurementQueryParameters : BaseQueryParameters
    {
        /// <summary>
        /// Filter by project ID
        /// </summary>
        public string? ProjectId { get; set; }

        /// <summary>
        /// Filter by asset ID
        /// </summary>
        public string? AssetId { get; set; }

        /// <summary>
        /// Filter by measurement type (e.g., "Ultrasonic Thickness", "Vibration")
        /// </summary>
        public string? MeasurementType { get; set; }

        /// <summary>
        /// Filter by location
        /// </summary>
        public string? Location { get; set; }

        /// <summary>
        /// Filter by measured date after
        /// </summary>
        public DateTime? MeasuredAfter { get; set; }

        /// <summary>
        /// Filter by measured date before
        /// </summary>
        public DateTime? MeasuredBefore { get; set; }

        public override Dictionary<string, string> ToDictionary()
        {
            var dict = base.ToDictionary();

            if (!string.IsNullOrEmpty(ProjectId))
                dict["project_id"] = ProjectId;

            if (!string.IsNullOrEmpty(AssetId))
                dict["asset_id"] = AssetId;

            if (!string.IsNullOrEmpty(MeasurementType))
                dict["measurement_type"] = MeasurementType;

            if (!string.IsNullOrEmpty(Location))
                dict["location"] = Location;

            if (MeasuredAfter.HasValue)
                dict["measured_after"] = MeasuredAfter.Value.ToString("yyyy-MM-ddTHH:mm:ssZ");

            if (MeasuredBefore.HasValue)
                dict["measured_before"] = MeasuredBefore.Value.ToString("yyyy-MM-ddTHH:mm:ssZ");

            return dict;
        }
    }

    /// <summary>
    /// Query parameters for filtering checklists
    /// </summary>
    public class ChecklistQueryParameters : BaseQueryParameters
    {
        /// <summary>
        /// Filter by project ID
        /// </summary>
        public string? ProjectId { get; set; }

        /// <summary>
        /// Filter by status (e.g., "Draft", "In Progress", "Completed")
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// Filter by template ID
        /// </summary>
        public string? TemplateId { get; set; }

        /// <summary>
        /// Filter by checklist name
        /// </summary>
        public string? Name { get; set; }

        public override Dictionary<string, string> ToDictionary()
        {
            var dict = base.ToDictionary();

            if (!string.IsNullOrEmpty(ProjectId))
                dict["project_id"] = ProjectId;

            if (!string.IsNullOrEmpty(Status))
                dict["status"] = Status;

            if (!string.IsNullOrEmpty(TemplateId))
                dict["template_id"] = TemplateId;

            if (!string.IsNullOrEmpty(Name))
                dict["name"] = Name;

            return dict;
        }
    }

    /// <summary>
    /// Query parameters for filtering users
    /// </summary>
    public class UserQueryParameters : BaseQueryParameters
    {
        /// <summary>
        /// Filter by email
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Filter by role
        /// </summary>
        public string? Role { get; set; }

        /// <summary>
        /// Filter by active status
        /// </summary>
        public bool? IsActive { get; set; }

        public override Dictionary<string, string> ToDictionary()
        {
            var dict = base.ToDictionary();

            if (!string.IsNullOrEmpty(Email))
                dict["email"] = Email;

            if (!string.IsNullOrEmpty(Role))
                dict["role"] = Role;

            if (IsActive.HasValue)
                dict["is_active"] = IsActive.Value.ToString().ToLower();

            return dict;
        }
    }

    /// <summary>
    /// Query parameters for filtering workspaces
    /// </summary>
    public class WorkspaceQueryParameters : BaseQueryParameters
    {
        /// <summary>
        /// Filter by workspace name
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Filter by active status
        /// </summary>
        public bool? IsActive { get; set; }

        public override Dictionary<string, string> ToDictionary()
        {
            var dict = base.ToDictionary();

            if (!string.IsNullOrEmpty(Name))
                dict["name"] = Name;

            if (IsActive.HasValue)
                dict["is_active"] = IsActive.Value.ToString().ToLower();

            return dict;
        }
    }
}
