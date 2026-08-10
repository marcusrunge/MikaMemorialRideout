namespace MarcusRunge.MikaMemorialRideout.Api.Contracts;

public sealed record PlanningStatusResponse(IReadOnlyList<PlanningStatusItemResponse> Items);
