namespace SolarTracker.Services;



public class StateProvider
{
    public StateProvider(
        AppSettings appSettings)
    {
        _autoEnabled = appSettings.Auto;
    }


    public event EventHandler? AutoEnabledChanged;
    private bool _autoEnabled;
    public bool AutoEnabled
    {
        get => _autoEnabled;
        set
        {
            if (_autoEnabled == value)
                return;
            //changed
            _autoEnabled = value;
            AutoEnabledChanged?.Invoke(this, EventArgs.Empty);
        }
    }


    public float AzimuthDegreePerSecond { get; set; }
    public float AzimuthWasteTime { get; set; }
    public float AltitudeDegreePerSecond { get; set; }
    public float AltitudeWasteTime { get; set; }



    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public Orientation? LastTargetOrientation { get; set; }
    public Orientation? CurrentOrientation { get; set; }
}
