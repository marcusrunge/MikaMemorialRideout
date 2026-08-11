namespace MarcusRunge.MikaMemorialRideout.Contracts;

public sealed record AdminRegistrationMutationResponse(
    string Status,
    string Message,
    AdminRegistrationResponse? Item = null);
