namespace MarcusRunge.MikaMemorialRideout.Contracts;

public sealed record PublicSummaryResponse(
    int PersonCount,
    int IndividualRegistrationCount,
    int GroupRegistrationCount,
    int OriginCount);
