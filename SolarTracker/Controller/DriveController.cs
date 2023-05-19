using Microsoft.AspNetCore.Mvc;


namespace SolarTracker.Controller;

[ApiController]
[Route("[controller]")]
public class DriveController : ControllerBase
{

    private readonly StateProvider _stateProvider;
    private readonly DriveService _driveService;
    private readonly IClock _clock;
    private readonly ILogger<DriveController> _logger;
    public DriveController(
        StateProvider stateProvider,
        DriveService driveService,
        IClock clock,
        ILogger<DriveController> logger)
    {
        _stateProvider = stateProvider;
        _driveService = driveService;
        _clock = clock;
        _logger = logger;
    }


    [HttpGet]
    [Route(nameof(GetLimits))]
    public LimitInfo GetLimits()
    {
        var result = new LimitInfo(
            _driveService.CheckLimit(DriveDirection.AzimuthNegative),
            _driveService.CheckLimit(DriveDirection.AzimuthPositive),
            _driveService.CheckLimit(DriveDirection.AltitudeNegative),
            _driveService.CheckLimit(DriveDirection.AltitudePositive),
            _clock.Now);

        _logger.LogDebug("GetLimits with result: {@result}", result);
        return result;
    }

    [HttpPost]
    [Route(nameof(DriveStartup))]
    public async Task<Orientation> DriveStartup(CancellationToken token)
    {
        _stateProvider.AutoEnabled = false;
        var result = await _driveService.DoStartupProcedure(token);
        _logger.LogInformation("DriveStartup with result: {@result}", result);
        return result;
    }

    [HttpPost]
    [Route(nameof(DriveNow))]
    public async Task<DriveResult> DriveNow(DriveDirection direction, double seconds, CancellationToken token)
    {
        _stateProvider.AutoEnabled = false;
        var result = await _driveService.Drive(
            direction,
            TimeSpan.FromSeconds(seconds),
            token);
        _logger.LogInformation("DriveNow with result: {@result}", result);
        return result;
    }

}