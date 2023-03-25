using Microsoft.AspNetCore.Mvc;

namespace SolarTracker.Controller;

[ApiController]
[Route("[controller]")]
public class AppController : ControllerBase
{
    private readonly StateProvider _stateProvider;
    private readonly ILogger<AppController> _logger;

    public AppController(
        StateProvider stateProvider,
        ILogger<AppController> logger)
    {
        _stateProvider = stateProvider;
        _logger = logger;
    }


    [HttpGet]
    public StateProvider Get()
    {
        _logger.LogInformation("AppState now {@appState}", _stateProvider);
        return _stateProvider;
    }


    [HttpPost]
    [Route(nameof(ChangeAutoMode))]
    public IActionResult ChangeAutoMode(bool enabled)
    {
        _stateProvider.AutoEnabled = enabled;
        _logger.LogInformation("AutoEnabled now {enabled}", enabled);
        return Ok();
    }
}