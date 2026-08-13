using MarcusRunge.MikaMemorialRideout.Api.Contracts;
using MarcusRunge.MikaMemorialRideout.Api.Security;
using MarcusRunge.MikaMemorialRideout.Api.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
namespace MarcusRunge.MikaMemorialRideout.Api.Functions;
internal sealed class RegistrationStateFunctions
{
    private const string AdminCodeHeader = "X-Admin-Code";
    private readonly IAdminCodeVerifier _verifier;
    private readonly IRegistrationStateRepository _repository;
    public RegistrationStateFunctions(IAdminCodeVerifier verifier, IRegistrationStateRepository repository) { _verifier = verifier; _repository = repository; }
    [Function("GetRegistrationState")]
    public async Task<IActionResult> GetAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "registration-state")] HttpRequest request, CancellationToken cancellationToken) => new OkObjectResult(await _repository.GetAsync(cancellationToken));
    [Function("UpdateRegistrationState")]
    public async Task<IActionResult> UpdateAsync([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "rideout-management/registration-state")] HttpRequest request, CancellationToken cancellationToken)
    {
        if (!_verifier.IsValid(request.Headers[AdminCodeHeader].FirstOrDefault())) return new UnauthorizedObjectResult(new AdminVerificationResponse("unauthorized", "Die Autorisierung ist fehlgeschlagen."));
        var input = await request.ReadFromJsonAsync<UpdateRegistrationStateRequest>(cancellationToken);
        return input is null ? new BadRequestObjectResult(new AdminOperationResponse("invalid", "Der gewünschte Status fehlt.")) : new OkObjectResult(await _repository.SetAsync(input.IsRegistrationOpen, cancellationToken));
    }
}
