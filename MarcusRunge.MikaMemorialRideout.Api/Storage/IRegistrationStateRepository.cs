using MarcusRunge.MikaMemorialRideout.Api.Contracts;
namespace MarcusRunge.MikaMemorialRideout.Api.Storage;
internal interface IRegistrationStateRepository
{
    Task<RegistrationStateResponse> GetAsync(CancellationToken cancellationToken);
    Task<RegistrationStateResponse> SetAsync(bool isRegistrationOpen, CancellationToken cancellationToken);
}
