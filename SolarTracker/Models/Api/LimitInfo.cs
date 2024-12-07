// ReSharper disable NotAccessedPositionalProperty.Global

namespace SolarTracker.Models.Api;

public record LimitInfo(
    bool AzimuthMinLimit,
    bool AzimuthMaxLimit,
    bool AltitudeMinLimit,
    bool AltitudeMaxLimit,
    DateTime Timestamp);