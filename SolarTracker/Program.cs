using Microsoft.AspNetCore.Builder;
using System.Globalization;

CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(CultureInfo.CurrentCulture.Name)
{
    NumberFormat = new NumberFormatInfo() //use . as comma
};
WebApplication.CreateBuilder(args)
    .Setup()
    .Build()
    .ConfigurePipeline()
    .Run();