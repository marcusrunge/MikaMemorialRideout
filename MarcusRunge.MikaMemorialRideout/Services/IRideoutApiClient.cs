using MarcusRunge.MikaMemorialRideout.Contracts;

namespace MarcusRunge.MikaMemorialRideout.Services;

public interface IRideoutApiClient
{
    Task<RegistrationStateResponse> GetRegistrationStateAsync(CancellationToken cancellationToken);
    Task<RegistrationStateResponse> UpdateRegistrationStateAsync(bool isOpen, string adminCode, CancellationToken cancellationToken);
    Task<AdminRegistrationMutationResponse> SetRegistrationRespondedAsync(string id, string version, bool isResponded, string adminCode, CancellationToken cancellationToken);
    Task<AdminOperationResponse> AnonymizeRegistrationsAsync(string adminCode, CancellationToken cancellationToken);
    Task<PlanningStatusResponse> GetPlanningStatusAsync(CancellationToken cancellationToken);
    Task<PublicSummaryResponse> GetPublicSummaryAsync(CancellationToken cancellationToken);
    Task<CreateRegistrationResponse> CreateRegistrationAsync(CreateRegistrationRequest request, CancellationToken cancellationToken);
    Task<UpdatePlanningStatusResponse> UpdatePlanningStatusAsync(string key, UpdatePlanningStatusRequest request, string adminCode, CancellationToken cancellationToken);
    Task<AdminVerificationResponse> VerifyAdminCodeAsync(string adminCode, CancellationToken cancellationToken);
    Task<AdminRegistrationsResponse> GetAdminRegistrationsAsync(string adminCode, CancellationToken cancellationToken);
    Task<AdminRegistrationMutationResponse> UpdateAdminRegistrationAsync(string id, AdminRegistrationUpdateRequest request, string adminCode, CancellationToken cancellationToken);
    Task<AdminRegistrationMutationResponse> DeleteAdminRegistrationAsync(string id, string version, string adminCode, CancellationToken cancellationToken);
}
