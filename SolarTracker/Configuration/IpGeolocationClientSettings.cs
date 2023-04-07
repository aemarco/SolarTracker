using aemarcoCommons.ToolboxAppOptions;
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace SolarTracker.Configuration;

public class IpGeolocationClientSettings : SettingsBase
{
    /// <summary>
    /// Api key to https://ipgeolocation.io/
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;
}
