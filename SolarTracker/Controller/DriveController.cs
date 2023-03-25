using Microsoft.AspNetCore.Mvc;


namespace SolarTracker.Controller;

[ApiController]
[Route("[controller]")]
public class DriveController : ControllerBase
{
    private readonly StateProvider _stateProvider;
    private readonly DriveService _driveService;
    private readonly IIoService _ioService;
    private readonly ILogger<DriveController> _logger;

    public DriveController(
        StateProvider stateProvider,
        DriveService driveService,
        IIoService ioService,
        ILogger<DriveController> logger)
    {
        _stateProvider = stateProvider;
        _driveService = driveService;
        _ioService = ioService;
        _logger = logger;
    }


    [HttpGet]
    [Route(nameof(GetLimits))]
    public LimitInfo GetLimits(CancellationToken token)
    {
        var result = new LimitInfo(
            _ioService.AzimuthMinLimit,
            _ioService.AzimuthMaxLimit,
            _ioService.AltitudeMinLimit,
            _ioService.AltitudeMaxLimit);
        _logger.LogInformation("GetLimits with result: {@result}", result);
        return result;
    }

    [HttpPost]
    [Route(nameof(DriveNow))]
    public async Task<DriveResult> DriveNow(DriveDirection direction, int time, CancellationToken token)
    {
        _stateProvider.AutoEnabled = false;
        var result = await _ioService.Drive(
            direction,
            TimeSpan.FromSeconds(time),
            token);
        _logger.LogInformation("DriveNow with result: {@result}", result);
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



}