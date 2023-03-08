using Microsoft.AspNetCore.Mvc;

namespace SolarTracker.Controller;

[ApiController]
[Route("[controller]")]
public class DriveController : ControllerBase
{
    private readonly AutoService _autoService;
    private readonly IIoService _ioService;
    public DriveController(
        AutoService autoService,
        IIoService ioService)
    {
        _autoService = autoService;
        _ioService = ioService;
    }


    [HttpPost]
    [Route(nameof(DriveNow))]
    public DriveResult DriveNow(DriveDirection direction, int time, CancellationToken token)
    {
        _autoService.AutoEnabled = false;

        var ts = TimeSpan.FromSeconds(time);
        var result = _ioService.Drive(direction, ts, token);
        return result;
    }
}