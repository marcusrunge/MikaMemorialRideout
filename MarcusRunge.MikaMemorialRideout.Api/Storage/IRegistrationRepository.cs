using MarcusRunge.MikaMemorialRideout.Api.Contracts;

namespace MarcusRunge.MikaMemorialRideout.Api.Storage;

internal interface IRegistrationRepository
{
    Task<RegistrationCreateResult> CreateAsync(CreateRegistrationRequest request, CancellationToken cancellationToken);
    Task<PublicSummaryResponse> GetPublicSummaryAsync(CancellationToken cancellationToken);
    Task<AdminRegistrationsResponse> GetAdminRegistrationsAsync(CancellationToken cancellationToken);
    Task<AdminRegistrationResponse?> GetAdminRegistrationAsync(string id, CancellationToken cancellationToken);
    Task<AdminRegistrationUpdateResult> UpdateAdminRegistrationAsync(string id, AdminRegistrationUpdateRequest request, CancellationToken cancellationToken);
    Task<AdminRegistrationMutationResponse> SetRespondedAsync(string id, string version, bool isResponded, CancellationToken cancellationToken);
    Task<AdminOperationResponse> AnonymizeAllAsync(CancellationToken cancellationToken);
    Task<AdminRegistrationDeleteResult> DeleteAdminRegistrationAsync(string id, string version, CancellationToken cancellationToken);
}
