namespace SolarTracker.Models.Api;
// ReSharper disable NotAccessedPositionalProperty.Global
// ReSharper disable UnusedMember.Global
public record ConfigInfo(
    AppSettings AppSettings,
    DeviceSettings DeviceSettings,
    DateTime Timestamp);