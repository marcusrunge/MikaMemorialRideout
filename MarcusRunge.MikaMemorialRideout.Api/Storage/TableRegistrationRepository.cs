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

        var registrationType = Normalize(request.RegistrationType);
        var name = request.Name.Trim();
        var groupName = string.IsNullOrWhiteSpace(request.GroupName) ? null : request.GroupName.Trim();
        var email = Normalize(request.Email);
        var duplicateName = registrationType == "group" ? Normalize(groupName!) : Normalize(name);

        var entity = new RegistrationEntity
        {
            PartitionKey = PartitionKey,
            RowKey = CreateDuplicateKey(registrationType, email, duplicateName),
            RegistrationType = registrationType,
            Name = name,
            GroupName = groupName,
            Email = email,
            Origin = request.Origin.Trim(),
            NormalizedOrigin = Normalize(request.Origin),
            PersonCount = request.PersonCount,
            Message = string.IsNullOrWhiteSpace(request.Message) ? null : request.Message.Trim(),
            CreatedAtUtc = _timeProvider.GetUtcNow()
        };

        try
        {
            // AddEntity is intentionally used instead of Upsert. A matching RowKey must never overwrite an existing registration.
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

        await foreach (var entity in _tableClient.QueryAsync<RegistrationEntity>(item => item.PartitionKey == PartitionKey, cancellationToken: cancellationToken))
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

    private static string Normalize(string value) => string.Join(' ', value.Trim().ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries));

    private static string CreateDuplicateKey(string registrationType, string email, string name)
    {
        var input = Encoding.UTF8.GetBytes($"{registrationType}|{email}|{name}");
        return Convert.ToHexString(SHA256.HashData(input)).ToLowerInvariant();
    }
}
