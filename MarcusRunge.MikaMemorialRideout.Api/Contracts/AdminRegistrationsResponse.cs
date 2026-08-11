namespace MarcusRunge.MikaMemorialRideout.Api.Contracts;

public sealed record AdminRegistrationsResponse(
    IReadOnlyList<AdminRegistrationResponse> Items,
    int RegistrationCount,
    int PersonCount);
