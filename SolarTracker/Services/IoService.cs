using aemarcoCommons.Extensions.NumberExtensions;
using System.Device.Gpio;
using System.Diagnostics;

namespace SolarTracker.Services;



public class IoService : IIoService
{
    private readonly DeviceSettings _deviceSettings;
    private readonly GpioController _controller;
    public IoService(
        DeviceSettings deviceSettings)
    {
        _deviceSettings = deviceSettings;
        _controller = new GpioController();
    }

    public DriveResult Drive(DriveDirection direction, TimeSpan timeToDrive, CancellationToken token)
    {
        var pin = direction switch
        {
            DriveDirection.AzimuthNegative => _deviceSettings.AzimuthDriveNegativePin,
            DriveDirection.AzimuthPositive => _deviceSettings.AzimuthDrivePositivePin,
            DriveDirection.AltitudeNegative => _deviceSettings.AltitudeDriveNegativePin,
            DriveDirection.AltitudePositive => _deviceSettings.AltitudeDrivePositivePin,
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
        };
        Func<bool> limitFunc = direction switch
        {
            DriveDirection.AzimuthNegative => () => AzimuthMinLimit,
            DriveDirection.AzimuthPositive => () => AzimuthMaxLimit,
            DriveDirection.AltitudeNegative => () => AltitudeMinLimit,
            DriveDirection.AltitudePositive => () => AltitudeMaxLimit,
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
        };
        bool StopFunc() => limitFunc() || token.IsCancellationRequested;


        var sw = Stopwatch.StartNew();
        Write(pin, true);
        SpinWait.SpinUntil(StopFunc, timeToDrive);
        Write(pin, false);
        sw.Stop();

        return new DriveResult(
            direction,
            TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds),
            limitFunc(),
            token.IsCancellationRequested);
    }

    public bool AzimuthMinLimit => Read(_deviceSettings.MinAzimuthLimitPin);
    public bool AzimuthMaxLimit => Read(_deviceSettings.MaxAzimuthLimitPin);
    public bool AltitudeMinLimit => Read(_deviceSettings.MinAltitudeLimitPin);
    public bool AltitudeMaxLimit => Read(_deviceSettings.MaxAltitudeLimitPin);



    private bool Read(int pinNumber)
    {
        return _controller.Read(pinNumber) == PinValue.High;
    }
    private void Write(int pinNumber, bool value)
    {
        _controller.Write(pinNumber, value ? PinValue.High : PinValue.Low);
    }
}





public class FakeIoService : IIoService
{



    public DriveResult Drive(
        DriveDirection direction,
        TimeSpan timeToDrive,
        CancellationToken token)
    {

        double driven = timeToDrive.TotalSeconds;
        var limit = false;

        if (direction is DriveDirection.AzimuthNegative)
        {
            _aziDriven -= timeToDrive.TotalSeconds;

            if (_aziDriven <= 0)
            {
                driven += _aziDriven;
                _aziDriven = 0;
            }
            limit = AzimuthMinLimit;

        }
        else if (direction is DriveDirection.AzimuthPositive)
        {
            _aziDriven += timeToDrive.TotalSeconds;

            if (_aziDriven >= 25)
            {
                driven -= _aziDriven - 25;
                _aziDriven = 25;
            }
            limit = AzimuthMaxLimit;

        }
        else if (direction is DriveDirection.AltitudeNegative)
        {
            _altDriven -= timeToDrive.TotalSeconds;
            if (_altDriven <= 0)
            {
                driven += _altDriven;
                _altDriven = 0;
            }
            limit = AltitudeMinLimit;

        }
        else if (direction is DriveDirection.AltitudePositive)
        {
            _altDriven += timeToDrive.TotalSeconds;

            if (_altDriven >= 25)
            {
                driven -= _altDriven - 25;
                _altDriven = 25;
            }
            limit = AltitudeMaxLimit;

        }

        return new DriveResult(
            direction,
            TimeSpan.FromSeconds(driven + 1),
            limit,
            token.IsCancellationRequested);
    }



    private double _aziDriven = 5;
    public bool AzimuthMinLimit => _aziDriven.IsNearlyEqual(0) || _aziDriven < 0;
    public bool AzimuthMaxLimit => _aziDriven.IsNearlyEqual(25) || _aziDriven > 25;


    private double _altDriven = 5;
    public bool AltitudeMinLimit => _altDriven.IsNearlyEqual(0) || _altDriven < 0;
    public bool AltitudeMaxLimit => _altDriven.IsNearlyEqual(25) || _altDriven > 25;
}