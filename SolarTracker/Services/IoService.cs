using aemarcoCommons.Extensions.NumberExtensions;
using System.Device.Gpio;
using System.Diagnostics;
namespace SolarTracker.Services;

public interface IIoService
{

    /// <summary>
    /// Move our device
    /// </summary>
    /// <param name="direction">direction to move</param>
    /// <param name="timeToDrive">time to move</param>
    /// <param name="token">abort</param>
    /// <returns>a drive result</returns>
    Task<DriveResult> Drive(DriveDirection direction, TimeSpan timeToDrive, CancellationToken token);

    /// <summary>
    /// currently min azimuth limit
    /// </summary>
    bool AzimuthMinLimit { get; }
    /// <summary>
    /// currently max azimuth limit
    /// </summary>
    bool AzimuthMaxLimit { get; }
    /// <summary>
    /// currently min altitude limit
    /// </summary>
    bool AltitudeMinLimit { get; }
    /// <summary>
    /// currently max altitude limit
    /// </summary>
    bool AltitudeMaxLimit { get; }

}

public class IoService : IIoService
{
    private readonly DeviceSettings _deviceSettings;
    private readonly IClock _clock;
    private readonly ILogger<IoService> _logger;
    private readonly GpioController _controller;
    public IoService(
        DeviceSettings deviceSettings,
        IClock clock,
        ILogger<IoService> logger)
    {
        _deviceSettings = deviceSettings;
        _clock = clock;
        _logger = logger;
        _controller = new GpioController();
    }

    //write
    public async Task<DriveResult> Drive(DriveDirection direction, TimeSpan timeToDrive, CancellationToken token)
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

        var sw = Stopwatch.StartNew();
        Write(pin, true);
        while (
            !token.IsCancellationRequested &&
            !limitFunc() &&
            sw.ElapsedMilliseconds < timeToDrive.TotalMilliseconds)
        {
            await Task.Delay(25, CancellationToken.None);
        }
        Write(pin, false);
        sw.Stop();

        await Task.Delay(2500, token);

        var result = new DriveResult(
            direction,
            TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds),
            limitFunc(),
            token.IsCancellationRequested,
            _clock.Now);

        _logger.LogInformation("Drive success with result: {@result}", result);

        return result;
    }
    private void Write(int pinNumber, bool value)
    {
        _controller.OpenPin(pinNumber, PinMode.Output);
        _controller.Write(pinNumber, value ? PinValue.High : PinValue.Low);
        _controller.ClosePin(pinNumber);
    }


    //read
    public bool AzimuthMinLimit => Read(_deviceSettings.MinAzimuthLimitPin);
    public bool AzimuthMaxLimit => Read(_deviceSettings.MaxAzimuthLimitPin);
    public bool AltitudeMinLimit => Read(_deviceSettings.MinAltitudeLimitPin);
    public bool AltitudeMaxLimit => Read(_deviceSettings.MaxAltitudeLimitPin);

    private bool Read(int pinNumber)
    {
        _controller.OpenPin(pinNumber, PinMode.InputPullDown);
        var result = _controller.Read(pinNumber) == PinValue.High;
        _controller.ClosePin(pinNumber);
        return result;
    }

}

public class FakeIoService : IIoService
{
    private readonly IClock _clock;

    public FakeIoService(
        IClock clock)
    {
        _clock = clock;
    }

    public Task<DriveResult> Drive(
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

        return Task.FromResult(new DriveResult(
            direction,
            TimeSpan.FromSeconds(driven + 1),
            limit,
            token.IsCancellationRequested,
            _clock.Now));
    }


    private double _aziDriven = 5;
    public bool AzimuthMinLimit => _aziDriven.IsNearlyEqual(0) || _aziDriven < 0;
    public bool AzimuthMaxLimit => _aziDriven.IsNearlyEqual(25) || _aziDriven > 25;


    private double _altDriven = 5;
    public bool AltitudeMinLimit => _altDriven.IsNearlyEqual(0) || _altDriven < 0;
    public bool AltitudeMaxLimit => _altDriven.IsNearlyEqual(25) || _altDriven > 25;
}