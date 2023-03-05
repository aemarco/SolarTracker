using Microsoft.AspNetCore.Mvc;

namespace SolarTracker.Controller;

[ApiController]
[Route("[controller]")]
public class ConfigController : ControllerBase
{
    private readonly AppSettings _appSettings;

    public ConfigController(AppSettings appSettings)
    {
        _appSettings = appSettings;
    }

    [HttpGet]
    public AppSettings Get()
    {
        return _appSettings;
    }
}