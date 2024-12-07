namespace SolarTracker.Services;

public class DriveService
{
    private readonly StateProvider _stateProvider;
    private readonly DeviceSettings _deviceSettings;
    private readonly AppSettings _appSettings;
    private readonly IIoService _ioService;
    private readonly IClock _clock;

    public DriveService(
        StateProvider stateProvider,
        DeviceSettings deviceSettings,
        AppSettings appSettings,
        IIoService ioService,
        IClock clock)
    {
        _stateProvider = stateProvider;
        _deviceSettings = deviceSettings;
        _appSettings = appSettings;
        _ioService = ioService;
        _clock = clock;
    }



    public async Task<Orientation> DoStartupProcedure(CancellationToken token)
    {
        //maybe drive down
        if (!_ioService.AltitudeMinLimit)
            _ = await _ioService.Drive(DriveDirection.AltitudeNegative, TimeSpan.FromMinutes(2), token)
                .ConfigureAwait(false);

        //maybe drive left
        if (!_ioService.AzimuthMinLimit)
            _ = await _ioService.Drive(DriveDirection.AzimuthNegative, TimeSpan.FromMinutes(2), token)
                .ConfigureAwait(false);



        //drive in 1 go (time for angle)
        var up = await _ioService.Drive(DriveDirection.AltitudePositive, TimeSpan.FromMinutes(2), token)
            .ConfigureAwait(false);
        if (!_ioService.AltitudeMaxLimit)
            throw new Exception("could not reach max altitude");
        _stateProvider.AltitudeDegreePerSecond = Convert.ToSingle((_deviceSettings.MaxAltitude - _deviceSettings.MinAltitude)
                                                                  / up.TimeDriven.TotalSeconds);

        //drive back in n go´s (time for start/stop)
        var downParts = new List<DriveResult>();
        while (!_ioService.AltitudeMinLimit && downParts.Count < 15)
        {
            var timeToDrive = up.TimeDriven.TotalSeconds / 5;
            var driven = await _ioService
                .Drive(DriveDirection.AltitudeNegative, TimeSpan.FromSeconds(timeToDrive), token)
                .ConfigureAwait(false);
            downParts.Add(driven);
        }
        if (!_ioService.AltitudeMinLimit)
            throw new Exception("could not reach min altitude");
        var downAltWasted = downParts.Sum(x => x.TimeDriven.TotalSeconds) - up.TimeDriven.TotalSeconds;
        _stateProvider.AltitudeWasteTime = Convert.ToSingle(downAltWasted / downParts.Count);



        //drive right in 1 go (time for angle)
        var right = await _ioService.Drive(DriveDirection.AzimuthPositive, TimeSpan.FromMinutes(2), token)
            .ConfigureAwait(false);
        if (!_ioService.AzimuthMaxLimit)
            throw new Exception("could not reach max azimuth");
        _stateProvider.AzimuthDegreePerSecond = Convert.ToSingle((_deviceSettings.MaxAzimuth - _deviceSettings.MinAzimuth)
                                                                 / right.TimeDriven.TotalSeconds);
        //drive back in n go´s (time for start/stop)
        var leftParts = new List<DriveResult>();
        while (!_ioService.AzimuthMinLimit && leftParts.Count < 30)
        {
            var timeToDrive = right.TimeDriven.TotalSeconds / 10;
            var driven = await _ioService
                .Drive(DriveDirection.AzimuthNegative, TimeSpan.FromSeconds(timeToDrive), token)
                .ConfigureAwait(false);
            leftParts.Add(driven);
        }
        if (!_ioService.AzimuthMinLimit)
            throw new Exception("could not reach min azimuth");
        var aziWasted = leftParts.Sum(x => x.TimeDriven.TotalSeconds) - right.TimeDriven.TotalSeconds;
        _stateProvider.AzimuthWasteTime = Convert.ToSingle(aziWasted / leftParts.Count);

        //here we should know how much is the drive integration delay,
        //and how much angle do we cover per time.
        //Therefor we can calculate later how much time to drive....

        //set current position.
        var result = new Orientation(
            _deviceSettings.MinAzimuth,
            _deviceSettings.MinAltitude,
            _clock.Now.Add(_appSettings.AutoInterval),
            _clock.Now);
        _stateProvider.CurrentOrientation = result;

        return result;
    }



    public async Task DriveToTarget(CancellationToken token)
    {
        var target = _stateProvider.LastTargetOrientation
                     ?? throw new Exception("No target to drive to");
        _stateProvider.CurrentOrientation ??= await DoStartupProcedure(token);

        await DriveAzimuth(target, token);
        await DriveAltitude(target, token);
        _stateProvider.CurrentOrientation = _stateProvider.CurrentOrientation with
        {
            ValidUntil = target.ValidUntil
        };
    }
    private async Task DriveAzimuth(Orientation target, CancellationToken token)
    {
        _ = _stateProvider.CurrentOrientation ?? throw new Exception("Can´t target drive while no reference position");

        var direction = target.Azimuth > _stateProvider.CurrentOrientation.Azimuth
            ? DriveDirection.AzimuthPositive
            : DriveDirection.AzimuthNegative;
        if (CheckLimit(direction))
            return; //already in limit

        var driveAngle = Math.Abs(target.Azimuth - _stateProvider.CurrentOrientation.Azimuth);
        if (driveAngle < _deviceSettings.AzimuthMinAngleForDrive)
            return; //less than threshold


        var time = driveAngle / _stateProvider.AzimuthDegreePerSecond;
        var timeWithWaste = time + _stateProvider.AzimuthWasteTime;

        _ = await Drive(
            direction,
            TimeSpan.FromSeconds(timeWithWaste),
            token,
            target.ValidUntil);
    }
    private async Task DriveAltitude(Orientation target, CancellationToken token)
    {
        _ = _stateProvider.CurrentOrientation ?? throw new Exception("Can´t target drive while no reference position");

        var direction = target.Altitude > _stateProvider.CurrentOrientation.Altitude
            ? DriveDirection.AltitudePositive
            : DriveDirection.AltitudeNegative;
        if (CheckLimit(direction))
            return; //already in limit

        var driveAngle = Math.Abs(target.Altitude - _stateProvider.CurrentOrientation.Altitude);
        if (driveAngle < _deviceSettings.AltitudeMinAngleForDrive)
            return; //less than threshold


        var time = driveAngle / _stateProvider.AltitudeDegreePerSecond;
        var timeWithWaste = time + _stateProvider.AltitudeWasteTime;

        _ = await Drive(
            direction,
            TimeSpan.FromSeconds(timeWithWaste),
            token,
            target.ValidUntil);
    }



    public async Task<DriveResult> Drive(
        DriveDirection direction,
        TimeSpan timeToDrive,
        CancellationToken token,
        DateTime? validUntil = null)
    {
        var result = await _ioService.Drive(
            direction,
            timeToDrive,
            token);

        UpdateCurrentOrientation(
            result,
            validUntil ?? _clock.Now.Add(_appSettings.AutoInterval));

        return result;
    }
    private void UpdateCurrentOrientation(DriveResult driven, DateTime validUntil)
    {
        if (_stateProvider.CurrentOrientation is null)
            return;
        // we correct the current orientation only if we have one already

        var (degreePerSecond, wasted) = driven.Direction switch
        {
            DriveDirection.AzimuthPositive => (_stateProvider.AzimuthDegreePerSecond, _stateProvider.AzimuthWasteTime),
            DriveDirection.AzimuthNegative => (_stateProvider.AzimuthDegreePerSecond, _stateProvider.AzimuthWasteTime),
            DriveDirection.AltitudePositive => (_stateProvider.AltitudeDegreePerSecond, _stateProvider.AltitudeWasteTime),
            DriveDirection.AltitudeNegative => (_stateProvider.AltitudeDegreePerSecond, _stateProvider.AltitudeWasteTime),
            _ => throw new ArgumentOutOfRangeException()
        };
        var degreeDriven = Convert.ToSingle(degreePerSecond * (driven.TimeDriven.TotalSeconds - wasted));

        var azi = _stateProvider.CurrentOrientation.Azimuth;
        azi = (driven.Direction, driven.LimitReached) switch
        {
            (DriveDirection.AzimuthPositive, true) => _deviceSettings.MaxAzimuth,
            (DriveDirection.AzimuthNegative, true) => _deviceSettings.MinAzimuth,
            (DriveDirection.AzimuthPositive, false) => Math.Clamp(azi + degreeDriven, _deviceSettings.MinAzimuth, _deviceSettings.MaxAzimuth),
            (DriveDirection.AzimuthNegative, false) => Math.Clamp(azi - degreeDriven, _deviceSettings.MinAzimuth, _deviceSettings.MaxAzimuth),
            _ => azi
        };
        var alt = _stateProvider.CurrentOrientation.Altitude;
        alt = (driven.Direction, driven.LimitReached) switch
        {
            (DriveDirection.AltitudePositive, true) => _deviceSettings.MaxAltitude,
            (DriveDirection.AltitudeNegative, true) => _deviceSettings.MinAltitude,
            (DriveDirection.AltitudePositive, false) => Math.Clamp(alt + degreeDriven, _deviceSettings.MinAltitude, _deviceSettings.MaxAltitude),
            (DriveDirection.AltitudeNegative, false) => Math.Clamp(alt - degreeDriven, _deviceSettings.MinAltitude, _deviceSettings.MaxAltitude),
            _ => alt
        };

        _stateProvider.CurrentOrientation = new Orientation(
            azi,
            alt,
            validUntil,
            _clock.Now);
    }

    public bool CheckLimit(DriveDirection direction)
    {
        var result = direction switch
        {
            DriveDirection.AzimuthNegative => _ioService.AzimuthMinLimit,
            DriveDirection.AzimuthPositive => _ioService.AzimuthMaxLimit,
            DriveDirection.AltitudeNegative => _ioService.AltitudeMinLimit,
            DriveDirection.AltitudePositive => _ioService.AltitudeMaxLimit,
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
        };
        return result;
    }

}