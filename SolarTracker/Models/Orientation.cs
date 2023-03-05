namespace SolarTracker.Models;

public record Orientation(float Azimuth, float Altitude, DateTime ValidUntil)
{
    public DateTime Timestamp { get; } = DateTime.Now;
}
