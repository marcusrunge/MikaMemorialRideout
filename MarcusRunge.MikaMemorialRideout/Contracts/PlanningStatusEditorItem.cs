using System.ComponentModel.DataAnnotations;

namespace MarcusRunge.MikaMemorialRideout.Contracts;

public sealed class PlanningStatusEditorItem
{
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public PlanningStatusLevel Level { get; set; }

    [Required(ErrorMessage = "Bitte gib einen Kurztext ein.")]
    [StringLength(120, MinimumLength = 2, ErrorMessage = "Der Kurztext muss zwischen 2 und 120 Zeichen lang sein.")]
    public string Summary { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Die Details dürfen höchstens 1.000 Zeichen lang sein.")]
    public string? Details { get; set; }

    public DateTimeOffset? UpdatedAtUtc { get; set; }
    public bool IsSaving { get; set; }
    public string? ResultMessage { get; set; }
    public string ResultClass { get; set; } = "admin-result";

    public static PlanningStatusEditorItem FromResponse(PlanningStatusItemResponse response) => new()
    {
        Key = response.Key,
        Title = response.Title,
        Level = response.Level,
        Summary = response.Summary,
        Details = response.Details,
        UpdatedAtUtc = response.UpdatedAtUtc
    };
}
