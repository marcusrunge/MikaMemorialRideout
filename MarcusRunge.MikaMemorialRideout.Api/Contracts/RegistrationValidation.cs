using System.Net.Mail;

namespace MarcusRunge.MikaMemorialRideout.Api.Contracts;

internal static class RegistrationValidation
{
    public static IReadOnlyDictionary<string, string[]> Validate(CreateRegistrationRequest request)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        var registrationType = request.RegistrationType.Trim().ToLowerInvariant();

        if (registrationType is not ("individual" or "group"))
            errors[nameof(request.RegistrationType)] = ["Erlaubt sind individual oder group."];

        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Trim().Length is < 2 or > 120)
            errors[nameof(request.Name)] = ["Der Name muss zwischen 2 und 120 Zeichen lang sein."];

        if (registrationType == "group" && (string.IsNullOrWhiteSpace(request.GroupName) || request.GroupName.Trim().Length is < 2 or > 120))
            errors[nameof(request.GroupName)] = ["Der Gruppenname muss zwischen 2 und 120 Zeichen lang sein."];

        if (!IsValidEmail(request.Email))
            errors[nameof(request.Email)] = ["Bitte gib eine gültige E-Mail-Adresse ein."];

        if (string.IsNullOrWhiteSpace(request.Origin) || request.Origin.Trim().Length is < 2 or > 120)
            errors[nameof(request.Origin)] = ["Die Herkunft muss zwischen 2 und 120 Zeichen lang sein."];

        if (request.PersonCount is < 1 or > 500)
            errors[nameof(request.PersonCount)] = ["Die Personenanzahl muss zwischen 1 und 500 liegen."];

        if (request.Message?.Trim().Length > 1000)
            errors[nameof(request.Message)] = ["Die Nachricht darf höchstens 1.000 Zeichen lang sein."];

        if (!request.PrivacyAccepted)
            errors[nameof(request.PrivacyAccepted)] = ["Die Datenschutzhinweise müssen akzeptiert werden."];
        if (!request.ParticipationTermsAccepted)
            errors[nameof(request.ParticipationTermsAccepted)] = ["Die Teilnahme- und Sicherheitshinweise müssen akzeptiert werden."];
        return errors;
    }

    private static bool IsValidEmail(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Trim().Length > 254)
            return false;

        try
        {
            var address = new MailAddress(value.Trim());
            return string.Equals(address.Address, value.Trim(), StringComparison.OrdinalIgnoreCase);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
