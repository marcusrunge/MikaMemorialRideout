using MarcusRunge.MikaMemorialRideout.Api.Contracts;
using MarcusRunge.MikaMemorialRideout.Api.Security;
using MarcusRunge.MikaMemorialRideout.Api.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace MarcusRunge.MikaMemorialRideout.Api.Functions;

internal sealed class AdminRegistrationFunctions
{
    private const string AdminCodeHeader = "X-Admin-Code";

    private readonly IAdminCodeVerifier _adminCodeVerifier;
    private readonly ILogger<AdminRegistrationFunctions> _logger;
    private readonly IRegistrationRepository _repository;

    public AdminRegistrationFunctions(
        IAdminCodeVerifier adminCodeVerifier,
        ILogger<AdminRegistrationFunctions> logger,
        IRegistrationRepository repository)
    {
        _adminCodeVerifier = adminCodeVerifier;
        _logger = logger;
        _repository = repository;
    }

    [Function("VerifyAdminCode")]
    public IActionResult Verify(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "rideout-management/verify")] HttpRequest request) =>
        IsAuthorized(request)
            ? new OkObjectResult(new AdminVerificationResponse("authorized", "Die Verwaltung wurde freigeschaltet."))
            : Unauthorized();

    [Function("GetAdminRegistrations")]
    public async Task<IActionResult> GetAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "rideout-management/registrations")] HttpRequest request,
        CancellationToken cancellationToken)
    {
        if (!IsAuthorized(request))
            return Unauthorized();

        return new OkObjectResult(await _repository.GetAdminRegistrationsAsync(cancellationToken));
    }

    [Function("UpdateAdminRegistration")]
    public async Task<IActionResult> UpdateAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "rideout-management/registrations/{id}")] HttpRequest request,
        string id,
        CancellationToken cancellationToken)
    {
        if (!IsAuthorized(request))
            return Unauthorized();

        AdminRegistrationUpdateRequest? input;

        try
        {
            input = await request.ReadFromJsonAsync<AdminRegistrationUpdateRequest>(cancellationToken);
        }
        catch (Exception exception) when (exception is BadHttpRequestException or System.Text.Json.JsonException)
        {
            return Invalid("Die übermittelten Anmeldedaten sind ungültig.");
        }

        if (input is null)
            return Invalid("Es wurden keine Anmeldedaten übermittelt.");

        var validationErrors = Validate(input);

        if (validationErrors.Count > 0)
        {
            return new BadRequestObjectResult(new
            {
                Status = "invalid",
                Message = "Bitte prüfe die eingegebenen Anmeldedaten.",
                Errors = validationErrors
            });
        }

        var result = await _repository.UpdateAdminRegistrationAsync(id, input, cancellationToken);

        return result.Status switch
        {
            AdminRegistrationUpdateStatus.Updated => new OkObjectResult(new AdminRegistrationMutationResponse("updated", "Die Anmeldung wurde aktualisiert.", result.Item)),
            AdminRegistrationUpdateStatus.NotFound => new NotFoundObjectResult(new AdminRegistrationMutationResponse("not_found", "Die Anmeldung ist nicht mehr vorhanden.")),
            AdminRegistrationUpdateStatus.Duplicate => new ConflictObjectResult(new AdminRegistrationMutationResponse("duplicate", "Für die geänderten Kontaktdaten besteht bereits eine Anmeldung.")),
            _ => new ConflictObjectResult(new AdminRegistrationMutationResponse("conflict", "Die Anmeldung wurde zwischenzeitlich geändert. Bitte lade die Liste neu."))
        };
    }

    [Function("DeleteAdminRegistration")]
    public async Task<IActionResult> DeleteAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "rideout-management/registrations/{id}")] HttpRequest request,
        string id,
        CancellationToken cancellationToken)
    {
        if (!IsAuthorized(request))
            return Unauthorized();

        var version = request.Query["version"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(version))
            return Invalid("Die Versionsangabe fehlt.");

        var result = await _repository.DeleteAdminRegistrationAsync(id, version, cancellationToken);

        if (result == AdminRegistrationDeleteResult.Deleted)
        {
            _logger.LogWarning("Registration {RegistrationId} was deleted through the admin interface.", id);
            return new OkObjectResult(new AdminRegistrationMutationResponse("deleted", "Die Anmeldung wurde gelöscht."));
        }

        return result == AdminRegistrationDeleteResult.NotFound
            ? new NotFoundObjectResult(new AdminRegistrationMutationResponse("not_found", "Die Anmeldung ist nicht mehr vorhanden."))
            : new ConflictObjectResult(new AdminRegistrationMutationResponse("conflict", "Die Anmeldung wurde zwischenzeitlich geändert. Bitte lade die Liste neu."));
    }

    private bool IsAuthorized(HttpRequest request) =>
        _adminCodeVerifier.IsValid(request.Headers[AdminCodeHeader].FirstOrDefault());

    private UnauthorizedObjectResult Unauthorized()
    {
        _logger.LogWarning("An admin registration request was rejected because authorization failed.");
        return new UnauthorizedObjectResult(new AdminVerificationResponse("unauthorized", "Die Autorisierung ist fehlgeschlagen."));
    }

    private static BadRequestObjectResult Invalid(string message) =>
        new(new AdminRegistrationMutationResponse("invalid", message));

    private static IReadOnlyDictionary<string, string[]> Validate(AdminRegistrationUpdateRequest request)
    {
        var createRequest = new CreateRegistrationRequest
        {
            RegistrationType = request.RegistrationType,
            Name = request.Name,
            GroupName = request.GroupName,
            Email = request.Email,
            Origin = request.Origin,
            PersonCount = request.PersonCount,
            Message = request.Message,
            PrivacyAccepted = true,
            Website = string.Empty
        };

        var errors = RegistrationValidation.Validate(createRequest)
            .ToDictionary(item => item.Key, item => item.Value, StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(request.Version))
            errors[nameof(request.Version)] = ["Die Versionsangabe fehlt."];

        return errors;
    }
}
