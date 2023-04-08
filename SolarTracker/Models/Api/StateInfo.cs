namespace SolarTracker.Models.Api;
// ReSharper disable NotAccessedPositionalProperty.Global
// ReSharper disable UnusedMember.Global

public record StateInfo(
    StateProvider State)
{
    public DateTime Timestamp { get; } = DateTime.Now;
}
