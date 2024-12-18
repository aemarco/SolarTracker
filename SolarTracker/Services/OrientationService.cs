﻿using Microsoft.EntityFrameworkCore;
using SolarTracker.Database;
using System.Net.NetworkInformation;

namespace SolarTracker.Services;

public interface IOrientationProvider
{
    /// <summary>
    /// delivers a new target orientation
    /// </summary>
    /// <param name="cancellationToken">cancellationToken</param>
    /// <returns>current target orientation</returns>
    Task SetTargetOrientation(CancellationToken cancellationToken);

}

public class OrientationService : IOrientationProvider
{

    private readonly ISunInfoProvider _sunInfoProvider;
    private readonly DeviceSettings _deviceSettings;
    private readonly SolarContextFactory _factory;
    private readonly AppSettings _appSettings;
    private readonly StateProvider _stateProvider;
    private readonly IClock _clock;
    private readonly ILogger<OrientationService> _logger;


    public OrientationService(
        ISunInfoProvider sunInfoProvider,
        DeviceSettings deviceSettings,
        SolarContextFactory factory,
        AppSettings appSettings,
        StateProvider stateProvider,
        IClock clock,
        ILogger<OrientationService> logger)
    {
        _sunInfoProvider = sunInfoProvider;
        _deviceSettings = deviceSettings;
        _factory = factory;
        _appSettings = appSettings;
        _stateProvider = stateProvider;
        _clock = clock;
        _logger = logger;
    }


    public async Task SetTargetOrientation(CancellationToken cancellationToken)
    {
        SunInfo? sunInfo;
        try
        {
            sunInfo = await _sunInfoProvider.GetSunInfo(
                _deviceSettings.Latitude,
                _deviceSettings.Longitude,
                cancellationToken);
            _stateProvider.SunProviderFallbackInfo = new SunProviderFallbackInfo(false, _clock.Now);

            await SetNewSunInfoForFallback(sunInfo, cancellationToken);
        }
        catch (Exception ex)
        {
            //TODO: change to async once Extensions.NetworkExtensions supports it 
            var onlineState = IsInternetAccessible() switch
            {
                true => "Internet Up",
                false => "Internet Down"
            };
            _stateProvider.SunProviderFallbackInfo = new SunProviderFallbackInfo(true, _clock.Now, onlineState, ex.Message);
            _logger.LogError(ex, "Failed to get SunInfo from SunInfoProvider with {@fallbackInfo}", _stateProvider.SunProviderFallbackInfo);

            sunInfo = await GetFallbackSunInfo(cancellationToken);
        }


        if (sunInfo is null)
            throw new Exception("Could not get any SunInfo");

        var result = CalculateTargetOrientation(sunInfo);
        _logger.LogInformation("Got new orientation target {@target}", result);
        _stateProvider.LastTargetOrientation = result;
    }

    private async Task SetNewSunInfoForFallback(SunInfo sunInfo, CancellationToken cancellationToken)
    {
        await using var ctx = _factory.Create();

        //delete entries which are not matching our position.
        ctx.SunInfos.RemoveRange(ctx.SunInfos
            .Where(x =>
                x.Latitude > _deviceSettings.Latitude + 0.01 ||
                x.Latitude < _deviceSettings.Latitude - 0.01 ||
                x.Longitude > _deviceSettings.Longitude + 0.01 ||
                x.Longitude < _deviceSettings.Longitude - 0.01));
        await ctx.SaveChangesAsync(cancellationToken);

        //delete oldest entries, keep only n entries
        ctx.SunInfos.RemoveRange(ctx.SunInfos
            .OrderByDescending(x => x.Timestamp)
            .Skip(600));
        await ctx.SaveChangesAsync(cancellationToken);

        //add new entry
        ctx.SunInfos.Add(sunInfo);
        await ctx.SaveChangesAsync(cancellationToken);
    }

    private static bool IsInternetAccessible()
    {
        try
        {
            using var ping = new Ping();
            var reply = ping.Send("8.8.8.8", 3000);
            return reply.Status == IPStatus.Success;
        }
        catch
        {
            return false;
        }
    }


    private async Task<SunInfo?> GetFallbackSunInfo(CancellationToken cancellationToken)
    {
        var currentTime = _clock.Now.TimeOfDay.TotalSeconds;
        await using var ctx = _factory.Create();
        var result = await ctx.SunInfos
            .AsNoTracking()
            .Where(x =>
                x.Latitude >= _deviceSettings.Latitude - 0.01 &&
                x.Latitude <= _deviceSettings.Latitude + 0.01 &&
                x.Longitude >= _deviceSettings.Longitude - 0.01 &&
                x.Longitude <= _deviceSettings.Longitude + 0.01)
            .OrderBy(x => Math.Abs(currentTime - x.SecondsOfDay))
            .FirstOrDefaultAsync(cancellationToken);

        return result;
    }

    private Orientation CalculateTargetOrientation(SunInfo sunInfo)
    {
        //we have following cases for azimuth / altitude
        // - before driving range, no sun  --> min / min
        // - before driving range          --> info / info (individually)
        // - driving range                 --> info / info (individually)
        // - after driving range           --> info / info (individually)
        // - after driving range, no sun   ---> min / min

        Orientation result;
        var time = _clock.TimeNow;

        if (time < sunInfo.Sunrise) //before driving range, no sun
        {
            var validUntil = _clock.DateNow.ToDateTime(sunInfo.Sunrise);
            result = new Orientation(_deviceSettings.MinAzimuth, _deviceSettings.MinAltitude, validUntil, _clock.Now);
        }
        else if (time >= sunInfo.Sunset) //after driving range, no sun
        {
            //sunrise does only fluctuate 1-2 min per day, so today sunrise is good enough for tomorrow
            var validUntil = _clock.DateNow.AddDays(1).ToDateTime(sunInfo.Sunrise);
            result = new Orientation(_deviceSettings.MinAzimuth, _deviceSettings.MinAltitude, validUntil, _clock.Now);
        }
        else //sun
        {
            var validUntil = _clock.Now.Add(_appSettings.AutoInterval);
            result = new Orientation(sunInfo.Azimuth, sunInfo.Altitude, validUntil, _clock.Now);
        }
        return result;
    }

}



