namespace MarcusRunge.MikaMemorialRideout.Api.Contracts;

public sealed record UpdatePlanningStatusResponse(string Status, string Message, PlanningStatusItemResponse? Item = null);
