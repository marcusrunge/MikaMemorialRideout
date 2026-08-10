namespace MarcusRunge.MikaMemorialRideout.Api.Security;

internal interface IAdminCodeVerifier
{
    bool IsValid(string? code);
}
