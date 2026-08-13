namespace MarcusRunge.MikaMemorialRideout.Api.Contracts;
public sealed record AdminRegistrationResponse(string Id, string Version, string RegistrationType, string Name, string? GroupName, string Email, string Origin, int PersonCount, string? Message, DateTimeOffset CreatedAtUtc, DateTimeOffset? RespondedAtUtc, DateTimeOffset? AnonymizedAtUtc);
