namespace SolarTracker.Models;

public record Orientation(float Azimuth, float Altitude)
{
    public DateTime Timestamp { get; } = DateTime.Now;
}
