using aemarcoCommons.Toolbox;
using aemarcoCommons.ToolboxAppOptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace SolarTracker;

public static class Bootstrapper
{
    public static WebApplicationBuilder Setup(this WebApplicationBuilder builder) =>
        builder
            .SetupConfiguration()
            .SetupLogging()
            .SetupServices();

    private static WebApplicationBuilder SetupConfiguration(this WebApplicationBuilder builder)
    {
        var sc = builder.Services;
        sc.AddConfigOptionsUtils(builder.Configuration, _ =>
        {

        });
        return builder;
    }

    private static WebApplicationBuilder SetupLogging(this WebApplicationBuilder builder)
    {
        var sc = builder.Services;
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .Enrich.WithThreadId()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentUserName()
            .CreateLogger();

        sc.AddLogging(loggingBuilder =>
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.AddSerilog(dispose: true);
        });
        return builder;
    }

    private static WebApplicationBuilder SetupServices(this WebApplicationBuilder builder)
    {
        var sc = builder.Services;

        sc.SetupToolbox();

        sc.AddHostedService<MainService>();
        sc.AddSingleton<AutoService>();



        sc.AddTransient<IOrientationProvider, OrientationService>();
        sc.AddHttpClient<ISunInfoProvider, IpGeolocationClient>();



        // Add services to the container.
        sc.AddControllers();

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        sc.AddEndpointsApiExplorer();
        sc.AddSwaggerGen();


        return builder;
    }




    public static WebApplication ConfigurePipeline(this WebApplication app)
    {

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseAuthorization();
        app.MapControllers();

        return app;
    }

}
