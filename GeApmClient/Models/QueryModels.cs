using System.Text.Json.Serialization;

namespace GeApmClient.Models
{
    /// <summary>
    /// Query parameters for building OData queries
    /// </summary>
    public class QueryParameters
    {
        /// <summary>
        /// $filter - Filter the results
        /// Example: "Status eq 'Active'"
        /// </summary>
        public string? Filter { get; set; }

        /// <summary>
        /// $select - Select specific properties
        /// Example: "EntityId,TML_ID,Status"
        /// </summary>
        public string? Select { get; set; }

        /// <summary>
        /// $expand - Expand related entities (depth: 1)
        /// Example: "Measurements"
        /// </summary>
        public string? Expand { get; set; }

        /// <summary>
        /// $orderby - Sort the results
        /// Example: "Measurement_Date desc"
        /// </summary>
        public string? OrderBy { get; set; }

        /// <summary>
        /// $top - Limit the number of results
        /// Example: 100
        /// </summary>
        public int? Top { get; set; }

        /// <summary>
        /// $skip - Skip a number of results (for pagination)
        /// Example: 100
        /// </summary>
        public int? Skip { get; set; }

        /// <summary>
        /// $count - Include total count of results
        /// Example: true
        /// </summary>
        public bool? Count { get; set; }

        /// <summary>
        /// Converts query parameters to OData query string
        /// </summary>
        public string ToQueryString()
        {
            var parameters = new List<string>();

            if (!string.IsNullOrWhiteSpace(Filter))
                parameters.Add($"$filter={Uri.EscapeDataString(Filter)}");

            if (!string.IsNullOrWhiteSpace(Select))
                parameters.Add($"$select={Uri.EscapeDataString(Select)}");

            if (!string.IsNullOrWhiteSpace(Expand))
                parameters.Add($"$expand={Uri.EscapeDataString(Expand)}");

            if (!string.IsNullOrWhiteSpace(OrderBy))
                parameters.Add($"$orderby={Uri.EscapeDataString(OrderBy)}");

            if (Top.HasValue)
                parameters.Add($"$top={Top.Value}");

            if (Skip.HasValue)
                parameters.Add($"$skip={Skip.Value}");

            if (Count.HasValue && Count.Value)
                parameters.Add("$count=true");

            return string.Join("&", parameters);
        }
    }

    /// <summary>
    /// Generic query result with pagination support
    /// </summary>
    /// <typeparam name="T">Type of entities in the result</typeparam>
    public class QueryResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int? TotalCount { get; set; }
        public string? NextLink { get; set; }
        public bool HasMore => !string.IsNullOrEmpty(NextLink);
    }
}
