using Microsoft.Extensions.Hosting;

namespace SolarTracker.Services;

public class MainService : IHostedService
{

    private readonly AutoService _autoService;
    private readonly ILogger<MainService> _logger;

    private readonly CancellationTokenSource _cts = new();
    public MainService(
        AutoService autoService,
        ILogger<MainService> logger)
    {
        _autoService = autoService;
        _logger = logger;

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
    private async Task MainLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                await _autoService.DoStuff(token)
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

}
