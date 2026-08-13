using System.ComponentModel.DataAnnotations;

namespace MarcusRunge.MikaMemorialRideout.Contracts;

public sealed class AdminRegistrationEditorItem
{
    public string Id { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;

    [Required]
    public string RegistrationType { get; set; } = string.Empty;

    [Required(ErrorMessage = "Bitte gib einen Namen ein.")]
    [StringLength(120, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(120)]
    public string? GroupName { get; set; }

    [Required(ErrorMessage = "Bitte gib eine E-Mail-Adresse ein.")]
    [EmailAddress(ErrorMessage = "Bitte gib eine gültige E-Mail-Adresse ein.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Bitte gib einen Herkunftsort ein.")]
    [StringLength(120, MinimumLength = 2)]
    public string Origin { get; set; } = string.Empty;

    [Range(1, 500)]
    public int PersonCount { get; set; }

    [StringLength(1000)]
    public string? Message { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? RespondedAtUtc { get; set; }
    public DateTimeOffset? AnonymizedAtUtc { get; set; }
    public bool IsSaving { get; set; }
    public bool IsDeleting { get; set; }
    public bool DeleteConfirmationVisible { get; set; }
    public bool IsExpanded { get; set; }
    public string? ResultMessage { get; set; }
    public string ResultClass { get; set; } = "admin-result";

    public static AdminRegistrationEditorItem FromResponse(AdminRegistrationResponse response) => new()
    {
        Id = response.Id,
        Version = response.Version,
        RegistrationType = response.RegistrationType,
        Name = response.Name,
        GroupName = response.GroupName,
        Email = response.Email,
        Origin = response.Origin,
        PersonCount = response.PersonCount,
        Message = response.Message,
        CreatedAtUtc = response.CreatedAtUtc,
        RespondedAtUtc = response.RespondedAtUtc,
        AnonymizedAtUtc = response.AnonymizedAtUtc
    };

    public void Apply(AdminRegistrationResponse response)
    {
        Id = response.Id;
        Version = response.Version;
        RegistrationType = response.RegistrationType;
        Name = response.Name;
        GroupName = response.GroupName;
        Email = response.Email;
        Origin = response.Origin;
        PersonCount = response.PersonCount;
        Message = response.Message;
        CreatedAtUtc = response.CreatedAtUtc;
        RespondedAtUtc = response.RespondedAtUtc;
        AnonymizedAtUtc = response.AnonymizedAtUtc;
    }
}