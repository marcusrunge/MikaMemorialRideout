using MarcusRunge.MikaMemorialRideout.Api.Contracts;

namespace MarcusRunge.MikaMemorialRideout.Api.Storage;

internal interface IRegistrationRepository
{
    Task<RegistrationCreateResult> CreateAsync(CreateRegistrationRequest request, CancellationToken cancellationToken);

    Task<PublicSummaryResponse> GetPublicSummaryAsync(CancellationToken cancellationToken);
}
