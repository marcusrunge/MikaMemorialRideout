using System.Security.Cryptography;
using System.Text;
using Azure;
using Azure.Data.Tables;
using MarcusRunge.MikaMemorialRideout.Api.Contracts;
using Microsoft.Extensions.Configuration;

namespace MarcusRunge.MikaMemorialRideout.Api.Storage;

internal sealed class TableRegistrationRepository : IRegistrationRepository
{
    private const string TableName = "RideoutRegistrations";
    private const string PartitionKey = "registration";

    private readonly TableClient _tableClient;
    private readonly TimeProvider _timeProvider;

    public TableRegistrationRepository(IConfiguration configuration, TimeProvider timeProvider)
    {
        var connectionString = configuration["RideoutStorageConnection"];

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Die Konfiguration RideoutStorageConnection fehlt.");

        _tableClient = new TableClient(connectionString, TableName);
        _timeProvider = timeProvider;
    }

    public async Task<RegistrationCreateResult> CreateAsync(CreateRegistrationRequest request, CancellationToken cancellationToken)
    {
        await _tableClient.CreateIfNotExistsAsync(cancellationToken);

        var entity = CreateEntity(request, _timeProvider.GetUtcNow());

        try
        {
            // Add statt Upsert bewahrt den atomaren Dublettenschutz auch bei parallelen Anfragen.
            await _tableClient.AddEntityAsync(entity, cancellationToken);
            return RegistrationCreateResult.Created;
        }
        catch (RequestFailedException exception) when (exception.Status == 409)
        {
            return RegistrationCreateResult.Duplicate;
        }
    }

    public async Task<PublicSummaryResponse> GetPublicSummaryAsync(CancellationToken cancellationToken)
    {
        await _tableClient.CreateIfNotExistsAsync(cancellationToken);

        var personCount = 0;
        var individualCount = 0;
        var groupCount = 0;
        var origins = new HashSet<string>(StringComparer.Ordinal);

        await foreach (var entity in QueryRegistrationsAsync(cancellationToken))
        {
            personCount = checked(personCount + entity.PersonCount);

            if (string.Equals(entity.RegistrationType, "group", StringComparison.Ordinal))
                groupCount++;
            else
                individualCount++;

            if (!string.IsNullOrWhiteSpace(entity.NormalizedOrigin))
                origins.Add(entity.NormalizedOrigin);
        }

        return new PublicSummaryResponse(personCount, individualCount, groupCount, origins.Count);
    }

    public async Task<AdminRegistrationsResponse> GetAdminRegistrationsAsync(CancellationToken cancellationToken)
    {
        await _tableClient.CreateIfNotExistsAsync(cancellationToken);

        var items = new List<AdminRegistrationResponse>();
        var personCount = 0;

        await foreach (var entity in QueryRegistrationsAsync(cancellationToken))
        {
            personCount = checked(personCount + entity.PersonCount);
            items.Add(ToAdminResponse(entity));
        }

        var orderedItems = items.OrderByDescending(item => item.CreatedAtUtc).ToArray();
        return new AdminRegistrationsResponse(orderedItems, orderedItems.Length, personCount);
    }

    public async Task<AdminRegistrationResponse?> GetAdminRegistrationAsync(string id, CancellationToken cancellationToken)
    {
        await _tableClient.CreateIfNotExistsAsync(cancellationToken);

        try
        {
            var response = await _tableClient.GetEntityAsync<RegistrationEntity>(PartitionKey, id, cancellationToken: cancellationToken);
            return ToAdminResponse(response.Value);
        }
        catch (RequestFailedException exception) when (exception.Status == 404)
        {
            return null;
        }
    }

    public async Task<AdminRegistrationUpdateResult> UpdateAdminRegistrationAsync(
        string id,
        AdminRegistrationUpdateRequest request,
        CancellationToken cancellationToken)
    {
        await _tableClient.CreateIfNotExistsAsync(cancellationToken);

        RegistrationEntity current;

        try
        {
            current = (await _tableClient.GetEntityAsync<RegistrationEntity>(PartitionKey, id, cancellationToken: cancellationToken)).Value;
        }
        catch (RequestFailedException exception) when (exception.Status == 404)
        {
            return new AdminRegistrationUpdateResult(AdminRegistrationUpdateStatus.NotFound);
        }

        if (!string.Equals(current.ETag.ToString(), request.Version, StringComparison.Ordinal))
            return new AdminRegistrationUpdateResult(AdminRegistrationUpdateStatus.Conflict);

        var updated = CreateEntity(request, current.CreatedAtUtc);

        try
        {
            if (string.Equals(updated.RowKey, current.RowKey, StringComparison.Ordinal))
            {
                updated.ETag = current.ETag;
                await _tableClient.UpdateEntityAsync(updated, current.ETag, TableUpdateMode.Replace, cancellationToken);
            }
            else
            {
                // Beide Operationen liegen in derselben Partition und werden deshalb atomar ausgeführt.
                // Bei einem bereits vorhandenen neuen Dublettenschlüssel schlägt die gesamte Transaktion fehl.
                var actions = new List<TableTransactionAction>
                {
                    new(TableTransactionActionType.Delete, current, current.ETag),
                    new(TableTransactionActionType.Add, updated)
                };

                await _tableClient.SubmitTransactionAsync(actions, cancellationToken);
            }
        }
        catch (RequestFailedException exception) when (exception.Status == 409)
        {
            return new AdminRegistrationUpdateResult(AdminRegistrationUpdateStatus.Duplicate);
        }
        catch (RequestFailedException exception) when (exception.Status == 404)
        {
            return new AdminRegistrationUpdateResult(AdminRegistrationUpdateStatus.NotFound);
        }
        catch (RequestFailedException exception) when (exception.Status == 412)
        {
            return new AdminRegistrationUpdateResult(AdminRegistrationUpdateStatus.Conflict);
        }

        var persisted = await GetAdminRegistrationAsync(updated.RowKey, cancellationToken);
        return new AdminRegistrationUpdateResult(AdminRegistrationUpdateStatus.Updated, persisted);
    }

    public async Task<AdminRegistrationDeleteResult> DeleteAdminRegistrationAsync(
        string id,
        string version,
        CancellationToken cancellationToken)
    {
        await _tableClient.CreateIfNotExistsAsync(cancellationToken);

        try
        {
            await _tableClient.DeleteEntityAsync(PartitionKey, id, new ETag(version), cancellationToken);
            return AdminRegistrationDeleteResult.Deleted;
        }
        catch (RequestFailedException exception) when (exception.Status == 404)
        {
            return AdminRegistrationDeleteResult.NotFound;
        }
        catch (RequestFailedException exception) when (exception.Status == 412)
        {
            return AdminRegistrationDeleteResult.Conflict;
        }
    }

    private AsyncPageable<RegistrationEntity> QueryRegistrationsAsync(CancellationToken cancellationToken) =>
        _tableClient.QueryAsync<RegistrationEntity>(item => item.PartitionKey == PartitionKey, cancellationToken: cancellationToken);

    private static RegistrationEntity CreateEntity(CreateRegistrationRequest request, DateTimeOffset createdAtUtc) =>
        CreateEntity(
            request.RegistrationType,
            request.Name,
            request.GroupName,
            request.Email,
            request.Origin,
            request.PersonCount,
            request.Message,
            createdAtUtc);

    private static RegistrationEntity CreateEntity(AdminRegistrationUpdateRequest request, DateTimeOffset createdAtUtc) =>
        CreateEntity(
            request.RegistrationType,
            request.Name,
            request.GroupName,
            request.Email,
            request.Origin,
            request.PersonCount,
            request.Message,
            createdAtUtc);

    private static RegistrationEntity CreateEntity(
        string registrationTypeValue,
        string nameValue,
        string? groupNameValue,
        string emailValue,
        string originValue,
        int personCount,
        string? messageValue,
        DateTimeOffset createdAtUtc)
    {
        var registrationType = Normalize(registrationTypeValue);
        var name = nameValue.Trim();
        var groupName = string.IsNullOrWhiteSpace(groupNameValue) ? null : groupNameValue.Trim();
        var email = Normalize(emailValue);
        var duplicateName = registrationType == "group" ? Normalize(groupName!) : Normalize(name);

        return new RegistrationEntity
        {
            PartitionKey = PartitionKey,
            RowKey = CreateDuplicateKey(registrationType, email, duplicateName),
            RegistrationType = registrationType,
            Name = name,
            GroupName = groupName,
            Email = email,
            Origin = originValue.Trim(),
            NormalizedOrigin = Normalize(originValue),
            PersonCount = personCount,
            Message = string.IsNullOrWhiteSpace(messageValue) ? null : messageValue.Trim(),
            CreatedAtUtc = createdAtUtc
        };
    }

    private static AdminRegistrationResponse ToAdminResponse(RegistrationEntity entity) =>
        new(
            entity.RowKey,
            entity.ETag.ToString(),
            entity.RegistrationType,
            entity.Name,
            entity.GroupName,
            entity.Email,
            entity.Origin,
            entity.PersonCount,
            entity.Message,
            entity.CreatedAtUtc);

    private static string Normalize(string value) =>
        string.Join(' ', value.Trim().ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries));

    private static string CreateDuplicateKey(string registrationType, string email, string name)
    {
        var input = Encoding.UTF8.GetBytes($"{registrationType}|{email}|{name}");
        return Convert.ToHexString(SHA256.HashData(input)).ToLowerInvariant();
    }
}
