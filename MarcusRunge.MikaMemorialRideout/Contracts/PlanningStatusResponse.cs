namespace MarcusRunge.MikaMemorialRideout.Contracts;

public sealed record PlanningStatusResponse(IReadOnlyList<PlanningStatusItemResponse> Items);
