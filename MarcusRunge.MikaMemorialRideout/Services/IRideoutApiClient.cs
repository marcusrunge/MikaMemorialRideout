using MarcusRunge.MikaMemorialRideout.Contracts;

namespace MarcusRunge.MikaMemorialRideout.Services;

public interface IRideoutApiClient
{
    Task<PlanningStatusResponse> GetPlanningStatusAsync(CancellationToken cancellationToken);
    Task<PublicSummaryResponse> GetPublicSummaryAsync(CancellationToken cancellationToken);
    Task<CreateRegistrationResponse> CreateRegistrationAsync(CreateRegistrationRequest request, CancellationToken cancellationToken);
    Task<UpdatePlanningStatusResponse> UpdatePlanningStatusAsync(string key, UpdatePlanningStatusRequest request, string adminCode, CancellationToken cancellationToken);
}
