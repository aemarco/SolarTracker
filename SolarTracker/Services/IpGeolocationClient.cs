using Newtonsoft.Json;

namespace SolarTracker.Services;

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
        var query = $"/astronomy?apiKey={_settings.ApiKey}&lat={latitude}&long={longitude}";
        var resp = await _httpClient.GetAsync(query, token);
        resp.EnsureSuccessStatusCode();

        var ar = await resp.Content.ReadAsAsync<AstroResponse>(token);
        var result = new SunInfo
        {
            Timestamp = DateOnly.Parse(ar.CurrentDate).ToDateTime(TimeOnly.Parse(ar.CurrentTime)),
            Latitude = ar.Location.Latitude,
            Longitude = ar.Location.Longitude,

            Sunrise = TimeOnly.Parse(ar.Sunrise),
            Sunset = TimeOnly.Parse(ar.Sunset),
            Altitude = ar.Altitude,
            Azimuth = ar.Azimuth
        };
        _logger.LogInformation("Got new sun info {@sunInfo}", result);
        return result;
    }


    // ReSharper disable ClassNeverInstantiated.Local
    // ReSharper disable UnusedAutoPropertyAccessor.Local
    private record AstroResponse
    {
        public AstroLocation Location { get; init; } = null!;
        [JsonProperty("date")]
        public string CurrentDate { get; init; } = null!;
        [JsonProperty("current_time")]
        public string CurrentTime { get; init; } = null!;
        public string Sunrise { get; init; } = null!;
        public string Sunset { get; init; } = null!;
        [JsonProperty("sun_status")]
        public string SunStatus { get; init; } = null!;
        [JsonProperty("solar_noon")]
        public string SolarNoon { get; init; } = null!;
        [JsonProperty("day_length")]
        public string DayLength { get; init; } = null!;
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
