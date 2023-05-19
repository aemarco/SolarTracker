// ReSharper disable NotAccessedPositionalProperty.Global
namespace SolarTracker.Models.Db;

public record SunProviderFallbackInfo(
    bool Active,
    DateTime Timestamp,
    string OnlineState = "Okay",
    string? Message = null);

