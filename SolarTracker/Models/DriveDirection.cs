namespace SolarTracker.Models;


/// <summary>
/// 0 => AzimuthNegative,
/// 1 => AzimuthPositive,
/// 2 => AltitudeNegative,
/// 3 => AltitudePositive
/// </summary>
public enum DriveDirection
{
    AzimuthNegative,
    AzimuthPositive,
    AltitudeNegative,
    AltitudePositive
}