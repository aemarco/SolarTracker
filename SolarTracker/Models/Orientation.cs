namespace SolarTracker.Models;
// ReSharper disable UnusedMember.Global

public record Orientation(
    float Azimuth,
    float Altitude,
    DateTime ValidUntil)
{
    public DateTime Timestamp { get; } = DateTime.Now;
}