namespace MarcusRunge.MikaMemorialRideout.Api.Contracts;
public sealed record RegistrationStateResponse(bool IsRegistrationOpen, DateTimeOffset? UpdatedAtUtc);
