using CliWrap;
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

                if (_stateProvider.CheckIfShutdownIsDue())
                {
                    await DoShutdown(source.Token)
                        .ConfigureAwait(false);
                    break;
                }
                if (_stateProvider.AutoEnabled)
                    await DoAuto(source.Token)
                        .ConfigureAwait(false);
                else
                    await DoAutoDisabled(source.Token)
                        .ConfigureAwait(false);
            }
            catch (OperationCanceledException ab)
            {
                if (token.IsCancellationRequested)
                    _logger.LogInformation(ab, "Main loop canceled");
                else if (_autoChangeSource.IsCancellationRequested)
                    _logger.LogInformation(ab, "Auto change cancel");
                else
                    _logger.LogWarning(ab, "Unknown cancellation");

                //on our token we fall out of loop anyway,
                //- otherwise we continue.
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Something went wrong....");
            }
        }
        _logger.LogDebug("Main loop ended");
    }



    private async Task DoAutoDisabled(CancellationToken token)
    {
        _logger.LogDebug("Wait until auto is enabled again");
        await Task.Delay(Timeout.Infinite, token)
            .ConfigureAwait(false);
    }

    private async Task DoAuto(CancellationToken token)
    {
        if (_stateProvider.CurrentOrientation?.ValidUntil is { } valid &&
            valid - DateTime.Now is { } timeToWait &&
            timeToWait > TimeSpan.Zero)
        {
            //wait for next due update
            _logger.LogInformation("Wait for {validUntil} ({timeSpan}) or auto mode disable", _stateProvider.CurrentOrientation.ValidUntil, timeToWait);
            await Task.Delay(timeToWait, token)
                .ConfigureAwait(false);
        }

        using var scope = _serviceProvider.CreateScope();

        //get target orientation
        var orientationProvider = scope.ServiceProvider.GetRequiredService<IOrientationProvider>();
        await orientationProvider.SetTargetOrientation(token)
            .ConfigureAwait(false);

        //trigger positioning service to drive as necessary
        var drive = scope.ServiceProvider.GetRequiredService<DriveService>();
        await drive.DriveToTarget(token)
            .ConfigureAwait(false);
    }

    private async Task DoShutdown(CancellationToken token)
    {
        _logger.LogInformation("Shutdown in 5 min...");
        await Task.Delay(TimeSpan.FromMinutes(5), token)
            .ConfigureAwait(false);
        var result = await Cli.Wrap("sudo")
            .WithArguments(b =>
            {
                b.Add("shutdown");
                b.Add("+1");
            })
            .ExecuteAsync(CancellationToken.None);
        _logger.LogInformation("Shutdown command issued with result {@result}", result);
    }

}
