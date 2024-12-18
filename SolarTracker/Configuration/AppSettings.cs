﻿// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace SolarTracker.Configuration;

public class AppSettings : ISettingsBase
{

    /// <summary>
    /// This decides if the app starts with auto mode enabled
    /// </summary>
    public bool Auto { get; set; }
    /// <summary>
    /// Interval, in which the target orientation gets updated (min 1min)
    /// </summary>
    public TimeSpan AutoInterval { get; set; }
    /// <summary>
    /// So that /swagger is enabled
    /// </summary>
    public bool EnableSwaggerUi { get; set; }
    /// <summary>
    /// So that we don´t try to read or set any pins where we can´t
    /// </summary>
    public bool EnableFakeIo { get; set; }
    /// <summary>
    /// So that the system receives a shutdown order at the end of the day
    /// </summary>
    public bool ShutdownAfterSunset { get; set; }

}

public class AppSettingsValidator : AbstractValidator<AppSettings>
{
    public AppSettingsValidator()
    {
        RuleFor(x => x.AutoInterval)
            .GreaterThanOrEqualTo(TimeSpan.FromMinutes(1));
    }

}