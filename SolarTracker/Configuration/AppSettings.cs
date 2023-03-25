﻿using aemarcoCommons.ToolboxAppOptions;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace SolarTracker.Configuration;

public class AppSettings : SettingsBase
{
    public bool Auto { get; set; }
    public TimeSpan AutoInterval { get; set; }

    public double AzimuthMinAngleForDrive { get; set; }
    public double AltitudeMinAngleForDrive { get; set; }

}