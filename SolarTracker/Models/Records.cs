namespace SolarTracker.Models;

// ReSharper disable NotAccessedPositionalProperty.Global
// ReSharper disable UnusedMember.Global


public record LimitInfo(
    bool AzimuthMinLimit,
    bool AzimuthMaxLimit,
    bool AltitudeMinLimit,
    bool AltitudeMaxLimit)
{
    public DateTime Timestamp { get; } = DateTime.Now;
}


public record ConfigInfo(
    AppSettings AppSettings,
    DeviceSettings DeviceSettings)
{
    public DateTime Timestamp { get; } = DateTime.Now;
}

public record DriveResult(
    DriveDirection Direction,
    TimeSpan TimeDriven,
    bool LimitReached,
    bool Aborted)
{
    public DateTime Timestamp { get; } = DateTime.Now;
}


public record Orientation(float Azimuth, float Altitude, DateTime ValidUntil)
{
    public DateTime Timestamp { get; } = DateTime.Now;
}