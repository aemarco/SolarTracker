namespace SolarTracker.Services;

public interface IOrientationProvider
{
    /// <summary>
    /// delivers a new target orientation
    /// </summary>
    /// <param name="cancellationToken">cancellationToken</param>
    /// <returns>current target orientation</returns>
    Task<Orientation> GetTargetOrientation(CancellationToken cancellationToken);

}

public class OrientationService : IOrientationProvider
{

    private readonly ISunInfoProvider _sunInfoProvider;
    private readonly DeviceSettings _deviceSettings;
    private readonly AppSettings _appSettings;
    private readonly ILogger<OrientationService> _logger;

    public OrientationService(
        ISunInfoProvider sunInfoProvider,
        DeviceSettings deviceSettings,
        AppSettings appSettings,
        ILogger<OrientationService> logger)
    {
        _sunInfoProvider = sunInfoProvider;
        _deviceSettings = deviceSettings;
        _appSettings = appSettings;
        _logger = logger;
    }

    public async Task<Orientation> GetTargetOrientation(CancellationToken cancellationToken)
    {
        var sunInfo = await _sunInfoProvider.GetSunInfo(_deviceSettings.Latitude, _deviceSettings.Longitude, cancellationToken);
        var result = CalculateTargetOrientation(sunInfo);
        _logger.LogInformation("Got new orientation target {@target}", result);
        return result;
    }

    private Orientation CalculateTargetOrientation(SunInfo sunInfo)
    {
        //we have following cases for azimuth / altitude
        // - before driving range, no sun  --> min / min
        // - before driving range          --> min / min (individually)
        // - driving range                 --> info / info (individually)
        // - after driving range           --> max / min (individually)
        // - after driving range, no sun   ---> min / min

        Orientation result;
        var time = TimeOnly.FromDateTime(DateTime.Now);
        if (time < sunInfo.Sunrise || //before driving range, no sun
            time > sunInfo.Sunset) //after driving range, no sun
        {
            //sunrise does only fluctuate 1-2 min per day, so today sunrise is good enough for tomorrow
            var validUntil = DateOnly.FromDateTime(DateTime.Now.AddDays(1)).ToDateTime(sunInfo.Sunrise);
            result = new Orientation(_deviceSettings.MinAzimuth, _deviceSettings.MinAltitude, validUntil);
        }
        else
        {//sun, so clamp in our range
            var validUntil = DateTime.Now.Add(_appSettings.AutoInterval);
            result = new Orientation(
                Math.Clamp(sunInfo.Azimuth, _deviceSettings.MinAzimuth, _deviceSettings.MaxAzimuth),
                Math.Clamp(sunInfo.Altitude, _deviceSettings.MinAltitude, _deviceSettings.MaxAltitude),
                validUntil);
        }
        return result;
    }

}



