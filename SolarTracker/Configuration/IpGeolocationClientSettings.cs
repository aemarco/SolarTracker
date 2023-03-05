using aemarcoCommons.ToolboxAppOptions;

namespace SolarTracker.Configuration;

public class IpGeolocationClientSettings : SettingsBase
{
    public string ApiKey { get; set; } = null!;
}
