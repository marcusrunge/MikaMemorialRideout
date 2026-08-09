using MarcusRunge.MikaMemorialRideout.Api.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace MarcusRunge.MikaMemorialRideout.Api.Functions;

internal sealed class PublicSummaryFunctions
{
    private readonly IRegistrationRepository _repository;

    public PublicSummaryFunctions(IRegistrationRepository repository)
    {
        _repository = repository;
    }

    [Function("GetPublicSummary")]
    public async Task<IActionResult> GetAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "public-summary")] HttpRequest request,
        CancellationToken cancellationToken)
    {
        var summary = await _repository.GetPublicSummaryAsync(cancellationToken);
        return new OkObjectResult(summary);
    }
}
