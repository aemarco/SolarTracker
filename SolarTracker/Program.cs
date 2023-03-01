using SolarTracker.Configuration;
using SolarTracker.Services;
using System;
using System.Globalization;


CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(CultureInfo.CurrentCulture.Name)
{
    NumberFormat = new NumberFormatInfo()
};

var appSettings = new AppSettings()
{

};

var s = new AstroApiService(appSettings);
var si = await s.GetSunInfo();


Console.WriteLine(si);