using aemarcoCommons.ToolboxAppOptions;

namespace SolarTracker.Configuration;
public class AppSettings : SettingsBase
{
    public string ApiKey { get; set; } = null!;
    public float Latitude { get; set; }
    public float Longitude { get; set; }


    public float MinAzimuth { get; set; }
    public float MaxAzimuth { get; set; }
    public float MinAltitude { get; set; }
    public float MaxAltitude { get; set; }




}
