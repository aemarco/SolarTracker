using CliWrap;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SolarTracker.Services;

public class MainService : BackgroundService
{

    private readonly StateProvider _stateProvider;
    private readonly AppSettings _appSettings;
    private readonly IServiceProvider _serviceProvider;
    private readonly IClock _clock;
    private readonly ILogger<MainService> _logger;
    public MainService(
        StateProvider stateProvider,
        AppSettings appSettings,
        IServiceProvider serviceProvider,
        IClock clock,
        ILogger<MainService> logger)
    {
        _stateProvider = stateProvider;
        _appSettings = appSettings;
        _serviceProvider = serviceProvider;
        _clock = clock;
        _logger = logger;

        _stateProvider.AutoEnabledChanged += (_, _) => _autoChangeSource.Cancel();
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Startup");
        return base.StartAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Graceful shutdown started");
        return base.StopAsync(cancellationToken);
    }


    private CancellationTokenSource _autoChangeSource = new();
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {

                if (_autoChangeSource.IsCancellationRequested)
                    _autoChangeSource = new CancellationTokenSource();
                var source = CancellationTokenSource.CreateLinkedTokenSource(
                    _autoChangeSource.Token,
                    stoppingToken);

                if (await CheckShutDownRoutine(source.Token))
                    break;


                if (_stateProvider.AutoEnabled)
                    await DoAuto(source.Token)
                        .ConfigureAwait(false);
                else
                    await DoAutoDisabled(source.Token)
                        .ConfigureAwait(false);
            }
            catch (OperationCanceledException ab)
            {
                if (stoppingToken.IsCancellationRequested)
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
            valid - _clock.Now is { } timeToWait &&
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



    /// <summary>
    /// Condition Check for shutdown initiation.
    /// If all conditions are met, shutdown is initiated, and result is true
    /// </summary>
    /// <param name="token"></param>
    /// <returns>true if a shutdown was initiated</returns>
    private async Task<bool> CheckShutDownRoutine(CancellationToken token)
    {
        //feature not enabled
        if (!_appSettings.ShutdownAfterSunset)
            return false;

        //there is no orientation info
        if (_stateProvider.CurrentOrientation is not { } co)
            return false;

        //valid until is not at least tomorrow
        if (DateOnly.FromDateTime(co.ValidUntil) <= _clock.DateNow)
            return false;


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
        return true;
    }

}
