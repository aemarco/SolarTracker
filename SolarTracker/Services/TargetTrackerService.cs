using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace SolarTracker.Services;

public class TargetTrackerService
{


    private readonly AstroApiClient _astroApiClient;
    private readonly ILogger<TargetTrackerService> _logger;

    public TargetTrackerService(
        AstroApiClient astroApiClient,
        ILogger<TargetTrackerService> logger)
    {
        _astroApiClient = astroApiClient;
        _logger = logger;
    }


    public async Task UpdateTarget(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var info = await _astroApiClient.GetSunInfo(cancellationToken);
        _logger.LogDebug("Got new sun info {@sunInfo}", info);
    }
}
