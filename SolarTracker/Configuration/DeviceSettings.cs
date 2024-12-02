// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace SolarTracker.Configuration;

public class DeviceSettings : ISettingsBase
{
    /// <summary>
    /// Gps Latitude of the device
    /// </summary>
    public float Latitude { get; set; }
    /// <summary>
    /// Gps Longitude of the device
    /// </summary>
    public float Longitude { get; set; }

    /// <summary>
    /// Minimum sway angle in degree, based on 0 = North
    /// </summary>
    public float MinAzimuth { get; set; }
    /// <summary>
    /// Io pin indicating minimum sway angle reached
    /// </summary>
    public int MinAzimuthLimitPin { get; set; }
    /// <summary>
    /// Io pin used to sway towards minimum/left
    /// </summary>
    public int AzimuthDriveNegativePin { get; set; }
    /// <summary>
    /// Maximum sway angle in degree, based on 0 = North
    /// </summary>
    public float MaxAzimuth { get; set; }
    /// <summary>
    /// Io pin indicating maximum sway angle reached
    /// </summary>
    public int MaxAzimuthLimitPin { get; set; }
    /// <summary>
    /// Io pin used to sway towards maximum/right
    /// </summary>
    public int AzimuthDrivePositivePin { get; set; }
    /// <summary>
    /// Sway-Threshold in degree, so that not every minor change in target orientation leads to driving operations
    /// </summary>
    public double AzimuthMinAngleForDrive { get; set; }

    /// <summary>
    /// Minimum tilt angle in degree, based on 0 = Horizontal
    /// </summary>
    public float MinAltitude { get; set; }
    /// <summary>
    /// Io pin indicating minimum tilt angle reached
    /// </summary>
    public int MinAltitudeLimitPin { get; set; }
    /// <summary>
    /// Io pin used to tilt towards minimum/down
    /// </summary>
    public int AltitudeDriveNegativePin { get; set; }
    /// <summary>
    /// Maximum tilt angle in degree, based on 0 = Horizontal
    /// </summary>
    public float MaxAltitude { get; set; }
    /// <summary>
    /// Io pin indicating maximum tilt angle reached
    /// </summary>
    public int MaxAltitudeLimitPin { get; set; }
    /// <summary>
    /// Io pin used to tilt towards maximum/up
    /// </summary>
    public int AltitudeDrivePositivePin { get; set; }
    /// <summary>
    /// Altitude-Threshold in degree, so that not every minor change in target orientation leads to driving operations
    /// </summary>
    public double AltitudeMinAngleForDrive { get; set; }

}