namespace SolarTracker.Services;



public class StateProvider
{
    public StateProvider(
        AppSettings appSettings)
    {
        _autoEnabled = appSettings.Auto;
        LoadDriveState();
    }

    //app
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

    //drive
    private void LoadDriveState()
    {

    }

    public float AzimuthDegreePerSecond { get; set; }
    public float AzimuthWasteTime { get; set; }
    public float AltitudeDegreePerSecond { get; set; }
    public float AltitudeWasteTime { get; set; }

    public Orientation? LastTargetOrientation { get; set; }
    public Orientation? CurrentOrientation { get; set; }


    public void SaveDriveState()
    {

    }



}
