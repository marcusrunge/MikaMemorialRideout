namespace MarcusRunge.MikaMemorialRideout.Contracts;

public sealed class CreateRegistrationRequest
{
    public string RegistrationType { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? GroupName { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Origin { get; set; } = string.Empty;
    public int PersonCount { get; set; } = 1;
    public string? Message { get; set; }
    public bool PrivacyAccepted { get; set; }
    public bool ParticipationTermsAccepted { get; set; }
    public string? Website { get; set; }
}
