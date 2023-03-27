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
    public float AltitudePosDegreePerSecond { get; set; }
    public float AltitudePosWasteTime { get; set; }
    public float AltitudeNegDegreePerSecond { get; set; }
    public float AltitudeNegWasteTime { get; set; }
    public Orientation? CurrentOrientation { get; set; }
}
