namespace MarcusRunge.MikaMemorialRideout.Api.Contracts;

public sealed class CreateRegistrationRequest
{
    public string RegistrationType { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? GroupName { get; set; }

    public string Email { get; set; } = string.Empty;

    public string Origin { get; set; } = string.Empty;

    public int PersonCount { get; set; }

    public string? Message { get; set; }

    public bool PrivacyAccepted { get; set; }

    // Bots tend to fill hidden fields. Real users never see or populate this value.
    public string? Website { get; set; }
}
