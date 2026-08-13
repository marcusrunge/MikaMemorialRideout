using Azure;
using Azure.Data.Tables;

namespace MarcusRunge.MikaMemorialRideout.Api.Storage;

internal sealed class RegistrationEntity : ITableEntity
{
    public string PartitionKey { get; set; } = "registration";

    public string RowKey { get; set; } = string.Empty;

    public DateTimeOffset? Timestamp { get; set; }

    public ETag ETag { get; set; }

    public string RegistrationType { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? GroupName { get; set; }

    public string Email { get; set; } = string.Empty;

    public string Origin { get; set; } = string.Empty;

    public string NormalizedOrigin { get; set; } = string.Empty;

    public int PersonCount { get; set; }

    public string? Message { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? RespondedAtUtc { get; set; }
    public DateTimeOffset? AnonymizedAtUtc { get; set; }
}
