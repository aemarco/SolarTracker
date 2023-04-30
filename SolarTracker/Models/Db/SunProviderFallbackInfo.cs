namespace SolarTracker.Models.Db;

public record SunProviderFallbackInfo(
    bool Active,
    // ReSharper disable once NotAccessedPositionalProperty.Global
    string? Message = null)
{
    public DateTime Timestamp { get; } = DateTime.Now;
}


