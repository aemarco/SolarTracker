using aemarcoCommons.ToolboxAppOptions;

namespace SolarTracker.Configuration;
public class AppSettings : SettingsBase
{
    public bool Auto { get; set; }
    public TimeSpan AutoInterval { get; set; }


}