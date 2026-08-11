using MarcusRunge.MikaMemorialRideout.Api.Contracts;

namespace MarcusRunge.MikaMemorialRideout.Api.Storage;

internal enum AdminRegistrationUpdateStatus
{
    Updated,
    NotFound,
    Conflict,
    Duplicate
}

internal sealed record AdminRegistrationUpdateResult(
    AdminRegistrationUpdateStatus Status,
    AdminRegistrationResponse? Item = null);
