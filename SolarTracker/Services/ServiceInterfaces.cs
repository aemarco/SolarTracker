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