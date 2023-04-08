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
    public StateInfo Get()
    {
        var result = new StateInfo(_stateProvider);
        _logger.LogDebug("AppState now {@appState}", result);
        return result;
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