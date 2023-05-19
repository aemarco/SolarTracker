namespace SolarTracker.Models.Api;
// ReSharper disable NotAccessedPositionalProperty.Global
// ReSharper disable UnusedMember.Global

public record LimitInfo(
    bool AzimuthMinLimit,
    bool AzimuthMaxLimit,
    bool AltitudeMinLimit,
    bool AltitudeMaxLimit,
    DateTime Timestamp);