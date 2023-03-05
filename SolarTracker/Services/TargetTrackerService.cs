namespace SolarTracker.Services;

public class TargetTrackerService
{

    private readonly AstroApiClient _astroApiClient;
    private readonly AppSettings _appSettings;
    private readonly ILogger<TargetTrackerService> _logger;

    public TargetTrackerService(
        AstroApiClient astroApiClient,
        AppSettings appSettings,
        ILogger<TargetTrackerService> logger)
    {
        _astroApiClient = astroApiClient;
        _appSettings = appSettings;
        _logger = logger;
    }


    public async Task<Orientation> GetTargetOrientation(CancellationToken cancellationToken)
    {
        var sunInfo = await _astroApiClient.GetSunInfo(cancellationToken);
        _logger.LogDebug("Got new sun info {@sunInfo}", sunInfo);

        var result = CalculateTargetOrientation(sunInfo);
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
            result = new Orientation(_appSettings.MinAzimuth, _appSettings.MinAltitude);
        }
        else
        {//sun, so clamp in our range
            result = new Orientation(
                Math.Clamp(sunInfo.Azimuth, _appSettings.MinAzimuth, _appSettings.MaxAzimuth),
                Math.Clamp(sunInfo.Altitude, _appSettings.MinAltitude, _appSettings.MaxAltitude));
        }
        return result;
    }

}



