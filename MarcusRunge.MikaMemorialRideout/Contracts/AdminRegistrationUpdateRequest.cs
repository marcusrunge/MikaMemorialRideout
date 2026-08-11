namespace MarcusRunge.MikaMemorialRideout.Contracts;

public sealed class AdminRegistrationUpdateRequest
{
    public string Version { get; set; } = string.Empty;
    public string RegistrationType { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? GroupName { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Origin { get; set; } = string.Empty;
    public int PersonCount { get; set; }
    public string? Message { get; set; }
}
