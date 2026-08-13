using Azure;
using Azure.Data.Tables;
namespace MarcusRunge.MikaMemorialRideout.Api.Storage;
internal sealed class RegistrationStateEntity : ITableEntity
{
    public string PartitionKey { get; set; } = "configuration";
    public string RowKey { get; set; } = "registration-state";
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
    public bool IsRegistrationOpen { get; set; } = true;
    public DateTimeOffset? UpdatedAtUtc { get; set; }
}
