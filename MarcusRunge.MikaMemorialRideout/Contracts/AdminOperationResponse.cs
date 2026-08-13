namespace MarcusRunge.MikaMemorialRideout.Contracts;
public sealed record AdminOperationResponse(string Status, string Message, int AffectedCount = 0);
