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

public interface IOrientationProvider
{
    /// <summary>
    /// delivers a new target orientation
    /// </summary>
    /// <param name="cancellationToken">cancellationToken</param>
    /// <returns>current target orientation</returns>
    Task<Orientation> GetTargetOrientation(CancellationToken cancellationToken);

}

public interface IIoService
{

    /// <summary>
    /// Move our device
    /// </summary>
    /// <param name="direction">direction to move</param>
    /// <param name="timeToDrive">time to move</param>
    /// <param name="token">abort</param>
    /// <returns>a drive result</returns>
    DriveResult Drive(DriveDirection direction, TimeSpan timeToDrive, CancellationToken token);

    /// <summary>
    /// currently min azimuth limit
    /// </summary>
    bool AzimuthMinLimit { get; }
    /// <summary>
    /// currently max azimuth limit
    /// </summary>
    bool AzimuthMaxLimit { get; }
    /// <summary>
    /// currently min altitude limit
    /// </summary>
    bool AltitudeMinLimit { get; }
    /// <summary>
    /// currently max altitude limit
    /// </summary>
    bool AltitudeMaxLimit { get; }
}