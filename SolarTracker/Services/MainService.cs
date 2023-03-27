using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SolarTracker.Services;

public class MainService : IHostedService
{

    private readonly StateProvider _stateProvider;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MainService> _logger;
    private readonly CancellationTokenSource _cts = new();
    public MainService(
        StateProvider stateProvider,
        IServiceProvider serviceProvider,
        ILogger<MainService> logger)
    {
        _stateProvider = stateProvider;
        _serviceProvider = serviceProvider;
        _logger = logger;

        _stateProvider.AutoEnabledChanged += (_, _) => _autoChangeSource.Cancel();
        _mainTask = Task.CompletedTask;
    }


    public Task StartAsync(CancellationToken _)
    {
        _logger.LogInformation("Startup");
        _mainTask = Task.Run(() => MainLoop(_cts.Token), _cts.Token);
        return Task.CompletedTask;
    }
    public async Task StopAsync(CancellationToken abortGraceFull)
    {
        _logger.LogInformation("Graceful shutdown started");
        _cts.Cancel();
        await Task.WhenAny(
            _mainTask,
            Task.Delay(Timeout.Infinite, abortGraceFull));
    }
    private Task _mainTask;
    private CancellationTokenSource _autoChangeSource = new();
    private async Task MainLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {

                if (_autoChangeSource.IsCancellationRequested)
                    _autoChangeSource = new CancellationTokenSource();
                var source = CancellationTokenSource.CreateLinkedTokenSource(
                    _autoChangeSource.Token,
                    token);

                if (_stateProvider.AutoEnabled)
                    await DoAuto(source.Token)
                        .ConfigureAwait(false);
                else
                    await DoAutoDisabled(source.Token)
                        .ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                //on our token we fall out of loop anyway, otherwise we continue.
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Something went wrong....");
            }
        }
    }




    private async Task DoAutoDisabled(CancellationToken token)
    {
        _logger.LogDebug("Wait until auto is enabled again");
        await Task.Delay(Timeout.Infinite, token)
            .ConfigureAwait(false);
    }

    private async Task DoAuto(CancellationToken token)
    {
        if (_stateProvider.CurrentOrientation is not null)
        {
            var timeToWait = _stateProvider.CurrentOrientation.ValidUntil - DateTime.Now;
            if (timeToWait > TimeSpan.Zero)
            {
                _logger.LogInformation("Wait for {validUntil} ({timeSpan}) or auto mode disable", _stateProvider.CurrentOrientation.ValidUntil, timeToWait);
                await Task.Delay(timeToWait, token)
                    .ConfigureAwait(false);
            }
        }

        using var scope = _serviceProvider.CreateScope();

        //get target orientation
        var orientationProvider = scope.ServiceProvider.GetRequiredService<IOrientationProvider>();
        var targetOrientation = await orientationProvider.GetTargetOrientation(token)
            .ConfigureAwait(false);

        //trigger positioning service to drive as necessary
        var drive = scope.ServiceProvider.GetRequiredService<DriveService>();
        await drive.DriveToTarget(targetOrientation, token)
            .ConfigureAwait(false);
    }

}
