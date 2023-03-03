﻿using Newtonsoft.Json;
using SolarTracker.Configuration;
using SolarTracker.Models;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SolarTracker.Services;
public class AstroApiService
{
    //https://github.com/IPGeolocation/ip-geolocation-api-dotnet-sdk

    private readonly AppSettings _settings;
    public AstroApiService(
        AppSettings settings)
    {
        _settings = settings;
    }


    public async Task<SunInfo> GetSunInfo()
    {
        var client = new HttpClient
        {
            BaseAddress = new Uri("https://api.ipgeolocation.io")
        };
        var query = $"/astronomy?apiKey={_settings.ApiKey}&lat={_settings.Latitude}&long={_settings.Longitude}";
        var resp = await client.GetAsync(query);
        resp.EnsureSuccessStatusCode();

        var ar = await resp.Content.ReadAsAsync<AstroResponse>();
        var result = new SunInfo
        {
            Timestamp = DateOnly.Parse(ar.CurrentDate).ToDateTime(TimeOnly.Parse(ar.CurrentTime)),
            Latitude = MathF.Round(ar.Location.Latitude, 3),
            Longitude = MathF.Round(ar.Location.Longitude, 3),

            Sunrise = TimeOnly.Parse(ar.Sunrise),
            Sunset = TimeOnly.Parse(ar.Sunset),
            Altitude = MathF.Round(ar.Altitude, 2),
            Azimuth = MathF.Round(ar.Azimuth, 2)
        };
        return result;
    }


    // ReSharper disable ClassNeverInstantiated.Local
    // ReSharper disable UnusedAutoPropertyAccessor.Local
    private record AstroResponse
    {
        public AstroLocation Location { get; init; } = null!;
        [JsonProperty("date")]
        public string CurrentDate { get; init; } = null!;
        [JsonProperty("current_time")]
        public string CurrentTime { get; init; } = null!;
        public string Sunrise { get; init; } = null!;
        public string Sunset { get; init; } = null!;
        [JsonProperty("sun_status")]
        public string SunStatus { get; init; } = null!;
        [JsonProperty("solar_noon")]
        public string SolarNoon { get; init; } = null!;
        [JsonProperty("day_length")]
        public string DayLength { get; init; } = null!;
        [JsonProperty("sun_altitude")]
        public float Altitude { get; init; }
        [JsonProperty("sun_distance")]
        public float Distance { get; init; }
        [JsonProperty("sun_azimuth")]
        public float Azimuth { get; init; }
    }

    private record AstroLocation(float Latitude, float Longitude);

    // ReSharper restore ClassNeverInstantiated.Local
    // ReSharper restore UnusedAutoPropertyAccessor.Local


}