using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace SolarTracker.Services;

public class MainService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MainService> _logger;

    private readonly CancellationTokenSource _cts = new();
    public MainService(
        IServiceProvider serviceProvider,
        ILogger<MainService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;

        _mainTask = Task.CompletedTask;
    }

    public Task StartAsync(CancellationToken _)
    {
        _mainTask = Task.Run(() => MainLoop(_cts.Token), _cts.Token);
        return Task.CompletedTask;
    }
    public async Task StopAsync(CancellationToken abortGraceFull)
    {
        _cts.Cancel();
        await Task.WhenAny(
            _mainTask,
            Task.Delay(Timeout.Infinite, abortGraceFull));
    }


    private Task _mainTask;
    private async Task MainLoop(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var scope = _serviceProvider.CreateScope();
            var sp = scope.ServiceProvider;
            try
            {
                //startup procedure...
                await Task.Delay(5000, cancellationToken);

                var targetService = sp.GetRequiredService<TargetTrackerService>();

                //update current target position
                await targetService.UpdateTarget(cancellationToken);

                //trigger positioning service to drive as necessary


                scope.Dispose();
            }
            catch (OperationCanceledException)
            {
                //cancel means anyway, that we fall out of loop...
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Something went wrong....");
            }
            finally
            {
                scope.Dispose();
            }
        }
    }

}
