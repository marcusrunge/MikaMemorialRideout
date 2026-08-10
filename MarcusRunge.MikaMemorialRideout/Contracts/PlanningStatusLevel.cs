using System.Text.Json.Serialization;

namespace MarcusRunge.MikaMemorialRideout.Contracts;

[JsonConverter(typeof(JsonStringEnumConverter<PlanningStatusLevel>))]
public enum PlanningStatusLevel
{
    Unknown,
    Green,
    Orange,
    Red
}
