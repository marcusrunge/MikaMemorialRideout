using MarcusRunge.MikaMemorialRideout.Api.Contracts;
using MarcusRunge.MikaMemorialRideout.Api.Security;
using MarcusRunge.MikaMemorialRideout.Api.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace MarcusRunge.MikaMemorialRideout.Api.Functions;

internal sealed class PlanningStatusFunctions
{
    private const string AdminCodeHeader = "X-Admin-Code";

    private readonly IAdminCodeVerifier _adminCodeVerifier;
    private readonly ILogger<PlanningStatusFunctions> _logger;
    private readonly IPlanningStatusRepository _repository;

    public PlanningStatusFunctions(
        IAdminCodeVerifier adminCodeVerifier,
        ILogger<PlanningStatusFunctions> logger,
        IPlanningStatusRepository repository)
    {
        _adminCodeVerifier = adminCodeVerifier;
        _logger = logger;
        _repository = repository;
    }

    [Function("GetPlanningStatus")]
    public async Task<IActionResult> GetAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "planning-status")] HttpRequest request,
        CancellationToken cancellationToken)
    {
        var status = await _repository.GetAllAsync(cancellationToken);
        return new OkObjectResult(status);
    }

    [Function("UpdatePlanningStatus")]
    public async Task<IActionResult> UpdateAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "planning-status/{key}")] HttpRequest request,
        string key,
        CancellationToken cancellationToken)
    {
        if (!_adminCodeVerifier.IsValid(request.Headers[AdminCodeHeader].FirstOrDefault()))
        {
            _logger.LogWarning("A planning status update was rejected because authorization failed.");
            return new UnauthorizedObjectResult(new UpdatePlanningStatusResponse("unauthorized", "Die Autorisierung ist fehlgeschlagen."));
        }

        if (!PlanningStatusCatalog.TryGet(key, out var definition))
            return new NotFoundObjectResult(new UpdatePlanningStatusResponse("not_found", "Der angeforderte Statusbereich ist nicht vorhanden."));

        UpdatePlanningStatusRequest? input;

        try
        {
            input = await request.ReadFromJsonAsync<UpdatePlanningStatusRequest>(cancellationToken);
        }
        catch (Exception exception) when (exception is BadHttpRequestException or System.Text.Json.JsonException)
        {
            return new BadRequestObjectResult(new UpdatePlanningStatusResponse("invalid", "Die übermittelten Statusdaten sind ungültig."));
        }

        if (input is null)
            return new BadRequestObjectResult(new UpdatePlanningStatusResponse("invalid", "Es wurden keine Statusdaten übermittelt."));

        var validationErrors = PlanningStatusValidation.Validate(input);

        if (validationErrors.Count > 0)
        {
            return new BadRequestObjectResult(new
            {
                Status = "invalid",
                Message = "Bitte prüfe die eingegebenen Statusdaten.",
                Errors = validationErrors
            });
        }

        var item = await _repository.UpdateAsync(definition, input, cancellationToken);
        _logger.LogInformation("Planning status {PlanningStatusKey} was updated.", definition.Key);
        return new OkObjectResult(new UpdatePlanningStatusResponse("updated", "Der Planungsstatus wurde aktualisiert.", item));
    }
}
