using MarcusRunge.MikaMemorialRideout.Api.Contracts;

namespace MarcusRunge.MikaMemorialRideout.Api.Storage;

internal sealed record PlanningStatusDefinition(
    string Key,
    string Title,
    PlanningStatusLevel DefaultLevel,
    string DefaultSummary,
    string? DefaultDetails = null);
