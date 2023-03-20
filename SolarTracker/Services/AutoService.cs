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
        }
    }


    private CancellationTokenSource _autoChangeSource = new();
    public async Task DoStuff(CancellationToken token)
    {
        if (_autoChangeSource.IsCancellationRequested)
            _autoChangeSource = new CancellationTokenSource();
        var source = CancellationTokenSource.CreateLinkedTokenSource(
            _autoChangeSource.Token,
            token);

        if (_appSettings.Auto)
            await DoAuto(source.Token)
                .ConfigureAwait(false);
        else
            await DoAutoDisabled(source.Token)
                .ConfigureAwait(false);
    }


    private async Task DoAutoDisabled(CancellationToken token)
    {
        _logger.LogDebug("Wait until auto is enabled again");
        await Task.Delay(Timeout.Infinite, token)
            .ConfigureAwait(false);
    }

    private async Task DoAuto(CancellationToken token)
    {
        using var scope = _serviceProvider.CreateScope();
        var drive = scope.ServiceProvider.GetRequiredService<DriveService>();

        if (CurrentOrientation is not null)
        {
            var timeToWait = CurrentOrientation.ValidUntil - DateTime.Now;
            if (timeToWait > TimeSpan.Zero)
            {
                _logger.LogDebug("Wait for {timeSpan} or auto mode disable", timeToWait);
                await Task.Delay(timeToWait, token)
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



    public Orientation? CurrentOrientation { get; private set; }
}
