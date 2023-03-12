using Microsoft.Extensions.DependencyInjection;

namespace SolarTracker.Services;

public class AutoService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly AppSettings _appSettings;
    private readonly ILogger<AutoService> _logger;

    public AutoService(
        IServiceProvider serviceProvider,
        AppSettings appSettings,
        ILogger<AutoService> logger)
    {
        _serviceProvider = serviceProvider;
        _appSettings = appSettings;
        _logger = logger;
    }


    public async Task DoStuff(CancellationToken token)
    {
        if (!_appSettings.Auto)
        {
            await DoAutoDisabled(token)
                .ConfigureAwait(false);
            return;
        }

        using var scope = _serviceProvider.CreateScope();
        var drive = scope.ServiceProvider.GetRequiredService<DriveService>();

        if (CurrentOrientation is not null)
        {
            var timeToWait = CurrentOrientation.ValidUntil - DateTime.Now;
            if (timeToWait > TimeSpan.Zero)
            {
                var source = CancellationTokenSource.CreateLinkedTokenSource(
                    _autoChangeSource.Token,
                    token);
                _logger.LogDebug("Wait for {timeSpan} or auto mode disable", timeToWait);

                await Task.Delay(timeToWait, source.Token)
                    .ConfigureAwait(false);
            }
        }

        //get target orientation
        var orientation = scope.ServiceProvider.GetRequiredService<IOrientationProvider>();
        var target = await orientation.GetTargetOrientation(token)
            .ConfigureAwait(false);

        //trigger positioning service to drive as necessary
        CurrentOrientation = await drive.DriveToTarget(CurrentOrientation, target, token)
            .ConfigureAwait(false);
    }


    public bool AutoEnabled
    {
        get => _appSettings.Auto;
        set
        {
            if (_appSettings.Auto == value)
                return;
            //changed

            _appSettings.Auto = value;

            _autoChangeSource.Cancel();
            _autoChangeSource = new CancellationTokenSource();
        }
    }


    private CancellationTokenSource _autoChangeSource = new();
    private async Task DoAutoDisabled(CancellationToken token)
    {
        var source = CancellationTokenSource.CreateLinkedTokenSource(
            _autoChangeSource.Token,
            token);
        _logger.LogDebug("Wait until auto is enabled again");
        await Task.Delay(Timeout.Infinite, source.Token)
            .ConfigureAwait(false);
    }


    public Orientation? CurrentOrientation { get; private set; }
}
