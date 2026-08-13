using Azure;
using Azure.Data.Tables;
using MarcusRunge.MikaMemorialRideout.Api.Contracts;
using Microsoft.Extensions.Configuration;
namespace MarcusRunge.MikaMemorialRideout.Api.Storage;
internal sealed class TableRegistrationStateRepository : IRegistrationStateRepository
{
    private const string TableName = "RideoutConfiguration";
    private readonly TableClient _tableClient;
    private readonly TimeProvider _timeProvider;
    public TableRegistrationStateRepository(IConfiguration configuration, TimeProvider timeProvider)
    {
        var connectionString = configuration["RideoutStorageConnection"];
        if (string.IsNullOrWhiteSpace(connectionString)) throw new InvalidOperationException("Die Konfiguration RideoutStorageConnection fehlt.");
        _tableClient = new TableClient(connectionString, TableName);
        _timeProvider = timeProvider;
    }
    public async Task<RegistrationStateResponse> GetAsync(CancellationToken cancellationToken)
    {
        await _tableClient.CreateIfNotExistsAsync(cancellationToken);
        try
        {
            var entity = (await _tableClient.GetEntityAsync<RegistrationStateEntity>("configuration", "registration-state", cancellationToken: cancellationToken)).Value;
            return new RegistrationStateResponse(entity.IsRegistrationOpen, entity.UpdatedAtUtc);
        }
        catch (RequestFailedException exception) when (exception.Status == 404)
        {
            return new RegistrationStateResponse(true, null);
        }
    }
    public async Task<RegistrationStateResponse> SetAsync(bool isRegistrationOpen, CancellationToken cancellationToken)
    {
        await _tableClient.CreateIfNotExistsAsync(cancellationToken);
        var entity = new RegistrationStateEntity { IsRegistrationOpen = isRegistrationOpen, UpdatedAtUtc = _timeProvider.GetUtcNow() };
        await _tableClient.UpsertEntityAsync(entity, TableUpdateMode.Replace, cancellationToken);
        return new RegistrationStateResponse(entity.IsRegistrationOpen, entity.UpdatedAtUtc);
    }
}
