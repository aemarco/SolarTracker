using SolarTracker.Database;

namespace SolarTracker.Services;


public class StateProvider
{

    private readonly SolarContextFactory _factory;
    private readonly IClock _clock;

    public StateProvider(
        AppSettings appSettings,
        SolarContextFactory factory,
        IClock clock)
    {
        _factory = factory;
        _clock = clock;
        _autoEnabled = appSettings.Auto;
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
    private float? _azimuthDegreePerSecond;
    public float AzimuthDegreePerSecond
    {
        get => _azimuthDegreePerSecond ??= _factory.Create().GetInfo<float>(nameof(AzimuthDegreePerSecond));
        set
        {
            _azimuthDegreePerSecond = value;
            using var ctx = _factory.Create().SetInfo(_azimuthDegreePerSecond, nameof(AzimuthDegreePerSecond));
        }
    }

    private float? _azimuthWasteTime;
    public float AzimuthWasteTime
    {
        get => _azimuthWasteTime ??= _factory.Create().GetInfo<float>(nameof(AzimuthWasteTime));
        set
        {
            _azimuthWasteTime = value;
            using var ctx = _factory.Create().SetInfo(_azimuthWasteTime, nameof(AzimuthWasteTime));
        }
    }

    private float? _altitudeDegreePerSecond;
    public float AltitudeDegreePerSecond
    {
        get => _altitudeDegreePerSecond ??= _factory.Create().GetInfo<float>(nameof(AltitudeDegreePerSecond));
        set
        {
            _altitudeDegreePerSecond = value;
            using var ctx = _factory.Create().SetInfo(_altitudeDegreePerSecond, nameof(AltitudeDegreePerSecond));
        }
    }

    private float? _altitudeWasteTime;
    public float AltitudeWasteTime
    {
        get => _altitudeWasteTime ??= _factory.Create().GetInfo<float>(nameof(AltitudeWasteTime));
        set
        {
            _altitudeWasteTime = value;
            using var ctx = _factory.Create().SetInfo(_altitudeWasteTime, nameof(AltitudeWasteTime));
        }
    }

    private Orientation? _currentOrientation;
    public Orientation? CurrentOrientation
    {
        get => _currentOrientation ??= _factory.Create().GetInfo<Orientation?>(nameof(CurrentOrientation));
        set
        {
            _currentOrientation = value;
            using var ctx = _factory.Create().SetInfo(_currentOrientation, nameof(CurrentOrientation));
        }
    }


    public Orientation? LastTargetOrientation { get; set; }






    private SunProviderFallbackInfo? _sunProviderFallbackInfo;
    public SunProviderFallbackInfo SunProviderFallbackInfo
    {
        get => _sunProviderFallbackInfo ??= _factory.Create().GetInfo<SunProviderFallbackInfo?>() ?? new SunProviderFallbackInfo(false, _clock.Now);
        set
        {
            if (_sunProviderFallbackInfo?.Active == value.Active)
                return;

            //only save a change when state has changed
            _sunProviderFallbackInfo = value;
            using var ctx = _factory.Create().SetInfo(_sunProviderFallbackInfo);
        }
    }

}
