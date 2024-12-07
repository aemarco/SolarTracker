// ReSharper disable NotAccessedPositionalProperty.Global

namespace SolarTracker.Models;

public record DriveResult(
    DriveDirection Direction,
    TimeSpan TimeDriven,
    bool LimitReached,
    bool Aborted,
    DateTime Timestamp);