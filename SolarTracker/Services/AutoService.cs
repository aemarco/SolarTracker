using Microsoft.Extensions.DependencyInjection;

namespace SolarTracker.Services;

public class AutoService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly AppSettings _appSettings;
    public AutoService(
        IServiceProvider serviceProvider,
        AppSettings appSettings)
    {
        _serviceProvider = serviceProvider;
        _appSettings = appSettings;
    }


    private bool _initial = true;
    public async Task DoStuff(CancellationToken token)
    {
        if (!_appSettings.Auto)
        {
            await DoAutoDisabled(token);
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
                ? _appSettings.AutoInterval
                : LastTargetOrientation.ValidUntil - DateTime.Now;
            var source = CancellationTokenSource.CreateLinkedTokenSource(
                _autoChangeSource.Token,
                token);
            await Task.Delay(timeToWait, source.Token);
        }

        //update current target orientation
        LastTargetOrientation = await orientation.GetTargetOrientation(token);


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
        await Task.Delay(Timeout.Infinite, source.Token);
    }



    public Orientation? LastTargetOrientation { get; private set; }


}
