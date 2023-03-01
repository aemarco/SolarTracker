using aemarcoCommons.ToolboxAppOptions;

namespace SolarTracker.Configuration;
public class AppSettings : SettingsBase
{
    public string ApiKey { get; set; } = null!;
    public float Latitude { get; set; }
    public float Longitude { get; set; }
}
