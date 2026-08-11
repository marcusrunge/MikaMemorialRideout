using MarcusRunge.MikaMemorialRideout.Contracts;

namespace MarcusRunge.MikaMemorialRideout.Services;

public interface IRideoutApiClient
{
    Task<PlanningStatusResponse> GetPlanningStatusAsync(CancellationToken cancellationToken);
    Task<PublicSummaryResponse> GetPublicSummaryAsync(CancellationToken cancellationToken);
    Task<CreateRegistrationResponse> CreateRegistrationAsync(CreateRegistrationRequest request, CancellationToken cancellationToken);
    Task<UpdatePlanningStatusResponse> UpdatePlanningStatusAsync(string key, UpdatePlanningStatusRequest request, string adminCode, CancellationToken cancellationToken);
    Task<AdminVerificationResponse> VerifyAdminCodeAsync(string adminCode, CancellationToken cancellationToken);
    Task<AdminRegistrationsResponse> GetAdminRegistrationsAsync(string adminCode, CancellationToken cancellationToken);
    Task<AdminRegistrationMutationResponse> UpdateAdminRegistrationAsync(string id, AdminRegistrationUpdateRequest request, string adminCode, CancellationToken cancellationToken);
    Task<AdminRegistrationMutationResponse> DeleteAdminRegistrationAsync(string id, string version, string adminCode, CancellationToken cancellationToken);
}
