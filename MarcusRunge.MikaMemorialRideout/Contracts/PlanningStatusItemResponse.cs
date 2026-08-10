namespace MarcusRunge.MikaMemorialRideout.Contracts;

public sealed record PlanningStatusItemResponse(
    string Key,
    string Title,
    PlanningStatusLevel Level,
    string Summary,
    string? Details,
    DateTimeOffset? UpdatedAtUtc);
