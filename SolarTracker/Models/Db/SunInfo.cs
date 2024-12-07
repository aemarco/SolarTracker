namespace SolarTracker.Models.Db;

public record SunInfo
{
    public DateTime Timestamp { get; init; }
    public float Latitude { get; init; }
    public float Longitude { get; init; }

    public TimeOnly Sunrise { get; init; }
    public TimeOnly Sunset { get; init; }
    public float Altitude { get; init; }
    public float Azimuth { get; init; }


    //helper property for query
    public double SecondsOfDay
    {
        get => Timestamp.TimeOfDay.TotalSeconds;
        // ReSharper disable once ValueParameterNotUsed
        init { }
    }
}
