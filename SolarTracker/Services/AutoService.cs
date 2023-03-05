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


    private bool _initial = true;
    public async Task DoStuff(CancellationToken token)
    {
        if (!_appSettings.Auto)
        {
            await DoAutoDisabled(token)
                .ConfigureAwait(false);
            return;
        }

        using var scope = _serviceProvider.CreateScope();
        var orientation = scope.ServiceProvider.GetRequiredService<IOrientationProvider>();


        if (_initial)
        {
            //startup procedure...
            _initial = false;
        }
        else
        {
            var timeToWait = LastTargetOrientation is null
                ? _appSettings.AutoInterval //on the happy path, that never happens
                : LastTargetOrientation.ValidUntil - DateTime.Now;
            var source = CancellationTokenSource.CreateLinkedTokenSource(
                _autoChangeSource.Token,
                token);
            _logger.LogDebug("Wait for {timeSpan} or auto mode disable", timeToWait);

            await Task.Delay(timeToWait, source.Token)
                .ConfigureAwait(false);
        }

        //update current target orientation
        LastTargetOrientation = await orientation.GetTargetOrientation(token)
            .ConfigureAwait(false);


        //trigger positioning service to drive as necessary

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

            if (_appSettings.Auto)
            {
                //we just enabled
                _initial = true;
            }

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



    public Orientation? LastTargetOrientation { get; private set; }


}
