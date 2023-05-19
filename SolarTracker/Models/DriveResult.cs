namespace SolarTracker.Models;
// ReSharper disable NotAccessedPositionalProperty.Global
// ReSharper disable UnusedMember.Global

public record DriveResult(
    DriveDirection Direction,
    TimeSpan TimeDriven,
    bool LimitReached,
    bool Aborted,
    DateTime Timestamp);