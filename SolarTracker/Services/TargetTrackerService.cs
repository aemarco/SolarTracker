using System.Diagnostics;
using System.Threading.Tasks;

namespace SolarTracker.Services;

public class TargetTrackerService
{


    private readonly AstroApiService _astroApiService;
    public TargetTrackerService(
        AstroApiService astroApiService)
    {
        _astroApiService = astroApiService;
    }


    public async Task UpdateTarget(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var info = await _astroApiService.GetSunInfo();

        Debug.WriteLine(info);


    }
}
