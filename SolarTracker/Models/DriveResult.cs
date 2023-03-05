namespace SolarTracker.Models;

public record DriveResult(DriveDirection Direction, TimeSpan TimeDriven, bool LimitReached, bool Aborted);