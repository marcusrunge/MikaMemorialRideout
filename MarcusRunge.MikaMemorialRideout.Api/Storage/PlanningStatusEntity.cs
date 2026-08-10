using Azure;
using Azure.Data.Tables;

namespace MarcusRunge.MikaMemorialRideout.Api.Storage;

internal sealed class PlanningStatusEntity : ITableEntity
{
    public string PartitionKey { get; set; } = "status";

    public string RowKey { get; set; } = string.Empty;

    public DateTimeOffset? Timestamp { get; set; }

    public ETag ETag { get; set; }

    public int Level { get; set; }

    public string Summary { get; set; } = string.Empty;

    public string? Details { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }
}
