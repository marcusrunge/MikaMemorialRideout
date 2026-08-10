using MarcusRunge.MikaMemorialRideout.Api.Contracts;

namespace MarcusRunge.MikaMemorialRideout.Api.Storage;

internal static class PlanningStatusCatalog
{
    public static readonly PlanningStatusDefinition[] Items =
    [
        new("overall", "Gesamtstatus", PlanningStatusLevel.Orange, "Planung läuft"),
        new("ordnungsamt", "Ordnungsamt", PlanningStatusLevel.Orange, "In Abstimmung"),
        new("strassenverkehrsamt", "Straßenverkehrsamt", PlanningStatusLevel.Orange, "In Abstimmung"),
        new("polizei", "Polizei", PlanningStatusLevel.Orange, "In Abstimmung"),
        new("organisation", "Organisation", PlanningStatusLevel.Orange, "Vorbereitungen laufen"),
        new("route", "Route", PlanningStatusLevel.Orange, "In Abstimmung"),
        new("treffpunkt", "Treffpunkt", PlanningStatusLevel.Orange, "Wird festgelegt"),
        new("road-marshals", "Road Marshals", PlanningStatusLevel.Orange, "Einteilung läuft"),
        new("notfallplanung", "Sanitäts- und Notfallplanung", PlanningStatusLevel.Orange, "Wird vorbereitet"),
        new("aufstellflaeche", "Park- und Aufstellfläche", PlanningStatusLevel.Orange, "In Abstimmung"),
        new("wetter", "Wetter", PlanningStatusLevel.Unknown, "Noch keine belastbare Einschätzung")
    ];

    private static readonly Dictionary<string, PlanningStatusDefinition> ItemsByKey =
        Items.ToDictionary(item => item.Key, StringComparer.Ordinal);

    public static bool TryGet(string key, out PlanningStatusDefinition definition) =>
        ItemsByKey.TryGetValue(key.Trim().ToLowerInvariant(), out definition!);
}
