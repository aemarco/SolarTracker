// ReSharper disable NotAccessedPositionalProperty.Global

namespace SolarTracker.Models.Api;

public record ConfigInfo(
    AppSettings AppSettings,
    DeviceSettings DeviceSettings,
    DateTime Timestamp);