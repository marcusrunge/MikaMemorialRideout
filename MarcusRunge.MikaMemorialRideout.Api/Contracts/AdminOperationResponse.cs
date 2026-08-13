namespace MarcusRunge.MikaMemorialRideout.Api.Contracts;
public sealed record AdminOperationResponse(string Status, string Message, int AffectedCount = 0);
