namespace MarcusRunge.MikaMemorialRideout.Contracts;
public sealed record RegistrationStateResponse(bool IsRegistrationOpen, DateTimeOffset? UpdatedAtUtc);
