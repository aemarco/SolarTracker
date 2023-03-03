using Microsoft.AspNetCore.Builder;
using SolarTracker;
using System.Globalization;

CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(CultureInfo.CurrentCulture.Name)
{
    NumberFormat = new NumberFormatInfo()
};
WebApplication.CreateBuilder(args)
    .SetupServices()
    .Build()
    .ConfigurePipeline()
    .Run();



