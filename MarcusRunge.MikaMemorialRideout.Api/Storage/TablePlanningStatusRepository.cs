using Azure.Data.Tables;
using MarcusRunge.MikaMemorialRideout.Api.Contracts;
using Microsoft.Extensions.Configuration;

namespace MarcusRunge.MikaMemorialRideout.Api.Storage;

internal sealed class TablePlanningStatusRepository : IPlanningStatusRepository
{
    private const string TableName = "RideoutPlanningStatus";
    private const string PartitionKey = "status";

    private readonly TableClient _tableClient;
    private readonly TimeProvider _timeProvider;

    public TablePlanningStatusRepository(IConfiguration configuration, TimeProvider timeProvider)
    {
        var connectionString = configuration["RideoutStorageConnection"];

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Die Konfiguration RideoutStorageConnection fehlt.");

        _tableClient = new TableClient(connectionString, TableName);
        _timeProvider = timeProvider;
    }

    public async Task<PlanningStatusResponse> GetAllAsync(CancellationToken cancellationToken)
    {
        await _tableClient.CreateIfNotExistsAsync(cancellationToken);

        var storedItems = new Dictionary<string, PlanningStatusEntity>(StringComparer.Ordinal);

        await foreach (var entity in _tableClient.QueryAsync<PlanningStatusEntity>(
                           item => item.PartitionKey == PartitionKey,
                           cancellationToken: cancellationToken))
        {
            storedItems[entity.RowKey] = entity;
        }

        // Defaults are returned in memory only. A build or first read never writes over previously maintained status data.
        var items = PlanningStatusCatalog.Items
            .Select(definition => storedItems.TryGetValue(definition.Key, out var entity)
                ? ToResponse(definition, entity)
                : ToDefaultResponse(definition))
            .ToArray();

        return new PlanningStatusResponse(items);
    }

    public async Task<PlanningStatusItemResponse> UpdateAsync(
        PlanningStatusDefinition definition,
        UpdatePlanningStatusRequest request,
        CancellationToken cancellationToken)
    {
        await _tableClient.CreateIfNotExistsAsync(cancellationToken);

        var entity = new PlanningStatusEntity
        {
            PartitionKey = PartitionKey,
            RowKey = definition.Key,
            Level = (int)request.Level,
            Summary = request.Summary.Trim(),
            Details = string.IsNullOrWhiteSpace(request.Details) ? null : request.Details.Trim(),
            UpdatedAtUtc = _timeProvider.GetUtcNow()
        };

        // Status data may be replaced deliberately. This repository has no access to the registration table.
        await _tableClient.UpsertEntityAsync(entity, TableUpdateMode.Replace, cancellationToken);
        return ToResponse(definition, entity);
    }

    private static PlanningStatusItemResponse ToResponse(PlanningStatusDefinition definition, PlanningStatusEntity entity) =>
        new(definition.Key, definition.Title, (PlanningStatusLevel)entity.Level, entity.Summary, entity.Details, entity.UpdatedAtUtc);

    private static PlanningStatusItemResponse ToDefaultResponse(PlanningStatusDefinition definition) =>
        new(definition.Key, definition.Title, definition.DefaultLevel, definition.DefaultSummary, definition.DefaultDetails, null);
}
