namespace MarcusRunge.MikaMemorialRideout.Api.Contracts;

public sealed class UpdatePlanningStatusRequest
{
    public PlanningStatusLevel Level { get; set; }

    public string Summary { get; set; } = string.Empty;

    public string? Details { get; set; }
}
