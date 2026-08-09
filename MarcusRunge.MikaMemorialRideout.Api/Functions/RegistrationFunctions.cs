using MarcusRunge.MikaMemorialRideout.Api.Contracts;
using MarcusRunge.MikaMemorialRideout.Api.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace MarcusRunge.MikaMemorialRideout.Api.Functions;

internal sealed class RegistrationFunctions
{
    private readonly ILogger<RegistrationFunctions> _logger;
    private readonly IRegistrationRepository _repository;

    public RegistrationFunctions(
        ILogger<RegistrationFunctions> logger,
        IRegistrationRepository repository)
    {
        _logger = logger;
        _repository = repository;
    }

    [Function("CreateRegistration")]
    public async Task<IActionResult> CreateAsync(
        [HttpTrigger(
            AuthorizationLevel.Anonymous,
            "post",
            Route = "registrations")]
        HttpRequest request,
        CancellationToken cancellationToken)
    {
        CreateRegistrationRequest? input;

        try
        {
            input = await request.ReadFromJsonAsync<CreateRegistrationRequest>(
                cancellationToken);
        }
        catch (Exception exception)
            when (exception is BadHttpRequestException
                  or System.Text.Json.JsonException)
        {
            return new BadRequestObjectResult(
                new CreateRegistrationResponse(
                    "invalid",
                    "Die übermittelten Daten sind ungültig."));
        }

        if (input is null)
        {
            return new BadRequestObjectResult(
                new CreateRegistrationResponse(
                    "invalid",
                    "Es wurden keine Anmeldedaten übermittelt."));
        }

        // Das versteckte Feld wird ausschließlich von Bots ausgefüllt.
        // Die Anfrage erhält absichtlich eine neutrale Erfolgsantwort,
        // damit automatisierte Absender den Schutz nicht erkennen können.
        if (!string.IsNullOrWhiteSpace(input.Website))
        {
            return new OkObjectResult(
                new CreateRegistrationResponse(
                    "accepted",
                    "Die Anmeldung wurde entgegengenommen."));
        }

        var validationErrors = RegistrationValidation.Validate(input);

        if (validationErrors.Count > 0)
        {
            return new BadRequestObjectResult(
                new
                {
                    Status = "invalid",
                    Message = "Bitte prüfe die eingegebenen Daten.",
                    Errors = validationErrors
                });
        }

        var result = await _repository.CreateAsync(
            input,
            cancellationToken);

        if (result == RegistrationCreateResult.Duplicate)
        {
            _logger.LogInformation(
                "A duplicate registration was rejected.");

            return new ConflictObjectResult(
                new CreateRegistrationResponse(
                    "duplicate",
                    "Für diese Kontaktdaten besteht bereits eine Anmeldung."));
        }

        _logger.LogInformation(
            "A new registration was stored successfully.");

        return new ObjectResult(
            new CreateRegistrationResponse(
                "created",
                "Die Anmeldung wurde gespeichert."))
        {
            StatusCode = StatusCodes.Status201Created
        };
    }
}