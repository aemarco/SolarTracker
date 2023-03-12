using Microsoft.AspNetCore.Mvc;

namespace SolarTracker.Controller;

[ApiController]
[Route("[controller]")]
public class DriveController : ControllerBase
{
    private readonly AutoService _autoService;
    private readonly DriveService _driveService;
    private readonly IIoService _ioService;
    public DriveController(
        AutoService autoService,
        DriveService driveService,
        IIoService ioService)
    {
        _autoService = autoService;
        _driveService = driveService;
        _ioService = ioService;
    }


    [HttpPost]
    [Route(nameof(DriveStartup))]
    public async Task<Orientation> DriveStartup(CancellationToken token)
    {
        _autoService.AutoEnabled = false;

        var result = await _driveService.DoStartupProcedure(token);
        return result;
    }


    [HttpPost]
    [Route(nameof(DriveNow))]
    public DriveResult DriveNow(DriveDirection direction, int time, CancellationToken token)
    {
        _autoService.AutoEnabled = false;

        var result = _ioService.Drive(
            direction,
            TimeSpan.FromSeconds(time),
            token);
        return result;
    }
}