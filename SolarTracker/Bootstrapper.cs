using aemarcoCommons.Toolbox;
using aemarcoCommons.ToolboxAppOptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace SolarTracker;

public static class Bootstrapper
{
    public static WebApplicationBuilder Setup(this WebApplicationBuilder builder) =>
        builder
            .SetupConfiguration()
            .SetupLogging()
            .SetupServices()
            .SetupApi();

    private static WebApplicationBuilder SetupConfiguration(this WebApplicationBuilder builder)
    {
        var sc = builder.Services;
        sc.AddConfigOptionsUtils(builder.Configuration);
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
        sc.AddSingleton<StateProvider>();


        sc.AddTransient<DriveService>();
        sc.AddTransient<IOrientationProvider, OrientationService>();
        sc.AddHttpClient<ISunInfoProvider, IpGeolocationClient>();
        var enableFakeIo = builder.Configuration.GetValue<bool>($"{nameof(AppSettings)}:{nameof(AppSettings.EnableFakeIo)}");
        sc.AddTransient(typeof(IIoService), enableFakeIo ? typeof(FakeIoService) : typeof(IoService));

        return builder;
    }
    private static WebApplicationBuilder SetupApi(this WebApplicationBuilder builder)
    {
        var sc = builder.Services;
        // Add services to the container.
        sc.AddControllers();

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        sc.AddEndpointsApiExplorer();
        sc.AddSwaggerGen();

        return builder;
    }




    public static WebApplication ConfigurePipeline(this WebApplication app)
    {

        var enableSwaggerUi = app.Configuration.GetValue<bool>($"{nameof(AppSettings)}:{nameof(AppSettings.EnableSwaggerUi)}");
        if (enableSwaggerUi)
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseAuthorization();
        app.MapControllers();

        return app;
    }

}
