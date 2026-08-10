namespace MarcusRunge.MikaMemorialRideout.Api.Contracts;

internal static class PlanningStatusValidation
{
    public static IReadOnlyDictionary<string, string[]> Validate(UpdatePlanningStatusRequest request)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

        if (!Enum.IsDefined(request.Level))
            errors[nameof(request.Level)] = ["Der Statuswert ist ungültig."];

        if (string.IsNullOrWhiteSpace(request.Summary) || request.Summary.Trim().Length is < 2 or > 120)
            errors[nameof(request.Summary)] = ["Der Kurztext muss zwischen 2 und 120 Zeichen lang sein."];

        if (request.Details?.Trim().Length > 1000)
            errors[nameof(request.Details)] = ["Die Details dürfen höchstens 1.000 Zeichen lang sein."];

        return errors;
    }
}
