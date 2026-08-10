using System.ComponentModel.DataAnnotations;

namespace MarcusRunge.MikaMemorialRideout.Contracts;

public sealed class IndividualRegistrationInput
{
    [Required(ErrorMessage = "Bitte gib deinen Namen ein.")]
    [StringLength(120, MinimumLength = 2, ErrorMessage = "Der Name muss zwischen 2 und 120 Zeichen lang sein.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Bitte gib deine E-Mail-Adresse ein.")]
    [EmailAddress(ErrorMessage = "Bitte gib eine gültige E-Mail-Adresse ein.")]
    [StringLength(254)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Bitte gib deinen Herkunftsort ein.")]
    [StringLength(120, MinimumLength = 2, ErrorMessage = "Der Herkunftsort muss zwischen 2 und 120 Zeichen lang sein.")]
    public string Origin { get; set; } = string.Empty;

    [Range(1, 500, ErrorMessage = "Die Personenanzahl muss zwischen 1 und 500 liegen.")]
    public int PersonCount { get; set; } = 1;

    [StringLength(1000, ErrorMessage = "Die Nachricht darf höchstens 1.000 Zeichen lang sein.")]
    public string? Message { get; set; }

    [Range(typeof(bool), "true", "true", ErrorMessage = "Bitte akzeptiere die Datenschutzhinweise.")]
    public bool PrivacyAccepted { get; set; }

    public string? Website { get; set; }
}
