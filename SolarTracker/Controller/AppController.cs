using Microsoft.AspNetCore.Mvc;

namespace SolarTracker.Controller;

[ApiController]
[Route("[controller]")]
public class AppController : ControllerBase
{

    private readonly AutoService _autoService;
    public AppController(
        AutoService autoService)
    {
        _autoService = autoService;
    }

    [HttpGet]
    [Route(nameof(GetOrientation))]
    public Orientation? GetOrientation()
    {
        return _autoService.CurrentOrientation;
    }

    [HttpPost]
    [Route(nameof(ChangeAutoMode))]
    public IActionResult ChangeAutoMode(bool enabled)
    {
        _autoService.AutoEnabled = enabled;
        return Ok();
    }

}