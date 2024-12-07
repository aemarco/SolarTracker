using Newtonsoft.Json;

namespace SolarTracker.Services;

public interface ISunInfoProvider
{
    /// <summary>
    /// delivers a new sun info for given geo coordinates
    /// </summary>
    /// <param name="latitude">latitude</param>
    /// <param name="longitude">longitude</param>
    /// <param name="token">cancellationToken</param>
    /// <returns>current sunInfo</returns>
    Task<SunInfo> GetSunInfo(float latitude, float longitude, CancellationToken token);
}



/// <summary>
/// client for https://ipgeolocation.io/
///
/// https://github.com/IPGeolocation/ip-geolocation-api-dotnet-sdk
/// unfortunately does not contain astro stuff
/// </summary>
public class IpGeolocationClient : ISunInfoProvider
{
    private readonly IpGeolocationClientSettings _settings;
    private readonly HttpClient _httpClient;
    private readonly ILogger<IpGeolocationClient> _logger;
    public IpGeolocationClient(
        IpGeolocationClientSettings settings,
        HttpClient httpClient,
        ILogger<IpGeolocationClient> logger)
    {
        _settings = settings;
        _httpClient = httpClient;
        _logger = logger;

        _httpClient.BaseAddress = new Uri("https://api.ipgeolocation.io");
    }


    /// <summary>
    /// delivers a new sun info for given geo coordinates
    /// </summary>
    /// <param name="latitude">latitude</param>
    /// <param name="longitude">longitude</param>
    /// <param name="token">cancellationToken</param>
    /// <returns>current sunInfo</returns>
    public async Task<SunInfo> GetSunInfo(float latitude, float longitude, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
            throw new Exception("IpGeolocationClient api key not configured");


        var query = $"/astronomy?apiKey={_settings.ApiKey}&lat={latitude}&long={longitude}";
        var resp = await _httpClient.GetAsync(query, token);
        resp.EnsureSuccessStatusCode();

        var astro = await resp.Content.ReadAsAsync<AstroResponse>(token);
        var result = new SunInfo
        {
            Timestamp = DateOnly.Parse(astro.CurrentDate).ToDateTime(TimeOnly.Parse(astro.CurrentTime)),
            Latitude = astro.Location.Latitude,
            Longitude = astro.Location.Longitude,

            Sunrise = TimeOnly.Parse(astro.Sunrise),
            Sunset = TimeOnly.Parse(astro.Sunset),
            Altitude = astro.Altitude,
            Azimuth = astro.Azimuth
        };
        _logger.LogInformation("Got new sun info {@sunInfo}", result);
        return result;
    }


    // ReSharper disable ClassNeverInstantiated.Local
    // ReSharper disable UnusedAutoPropertyAccessor.Local
    private record AstroResponse
    {
        public required AstroLocation Location { get; init; }
        [JsonProperty("date")]
        public required string CurrentDate { get; init; }
        [JsonProperty("current_time")]
        public required string CurrentTime { get; init; }
        public required string Sunrise { get; init; }
        public required string Sunset { get; init; }
        [JsonProperty("sun_status")]
        public required string SunStatus { get; init; }
        [JsonProperty("solar_noon")]
        public required string SolarNoon { get; init; }
        [JsonProperty("day_length")]
        public required string DayLength { get; init; }
        [JsonProperty("sun_altitude")]
        public float Altitude { get; init; }
        [JsonProperty("sun_distance")]
        public float Distance { get; init; }
        [JsonProperty("sun_azimuth")]
        public float Azimuth { get; init; }
    }

    private record AstroLocation(float Latitude, float Longitude);

    // ReSharper restore ClassNeverInstantiated.Local
    // ReSharper restore UnusedAutoPropertyAccessor.Local
}
