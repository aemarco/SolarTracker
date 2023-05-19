namespace SolarTracker.Models.Db;

public record SunProviderFallbackInfo(
    bool Active,
    DateTime Timestamp,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    string? Message = null);

