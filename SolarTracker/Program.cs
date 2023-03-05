using Microsoft.AspNetCore.Builder;
using System.Globalization;

CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(CultureInfo.CurrentCulture.Name)
{
    NumberFormat = new NumberFormatInfo()
};
WebApplication.CreateBuilder(args)
    .Setup()
    .Build()
    .ConfigurePipeline()
    .Run();



