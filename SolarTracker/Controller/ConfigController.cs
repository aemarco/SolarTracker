using Microsoft.AspNetCore.Mvc;

namespace SolarTracker.Controller;

[ApiController]
[Route("[controller]")]
public class ConfigController : ControllerBase
{
    private readonly AppSettings _appSettings;
    private readonly DeviceSettings _deviceSettings;
    private readonly IClock _clock;

    public ConfigController(
        AppSettings appSettings,
        DeviceSettings deviceSettings,
        IClock clock)
    {
        _appSettings = appSettings;
        _deviceSettings = deviceSettings;
        _clock = clock;
    }

    [HttpGet]
    public ConfigInfo Get()
    {
        var result = new ConfigInfo(_appSettings, _deviceSettings, _clock.Now);
        return result;
    }
}