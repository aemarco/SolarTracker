using aemarcoCommons.ToolboxAppOptions;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace SolarTracker.Configuration;

public class DeviceSettings : SettingsBase
{
    public float Latitude { get; set; }
    public float Longitude { get; set; }


    public float MinAzimuth { get; set; }
    public int MinAzimuthLimitPin { get; set; }
    public int AzimuthDriveNegativePin { get; set; }

    public float MaxAzimuth { get; set; }
    public int MaxAzimuthLimitPin { get; set; }
    public int AzimuthDrivePositivePin { get; set; }


    public float MinAltitude { get; set; }
    public int MinAltitudeLimitPin { get; set; }
    public int AltitudeDriveNegativePin { get; set; }

    public float MaxAltitude { get; set; }
    public int MaxAltitudeLimitPin { get; set; }
    public int AltitudeDrivePositivePin { get; set; }

}