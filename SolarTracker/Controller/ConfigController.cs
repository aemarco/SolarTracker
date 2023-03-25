using Microsoft.AspNetCore.Mvc;

namespace SolarTracker.Controller;

[ApiController]
[Route("[controller]")]
public class ConfigController : ControllerBase
{
    private readonly AppSettings _appSettings;
    private readonly DeviceSettings _deviceSettings;

    public ConfigController(
        AppSettings appSettings,
        DeviceSettings deviceSettings)
    {
        _appSettings = appSettings;
        _deviceSettings = deviceSettings;
    }

    [HttpGet]
    public ConfigInfo Get()
    {
        var result = new ConfigInfo(_appSettings, _deviceSettings);
        return result;
    }
}