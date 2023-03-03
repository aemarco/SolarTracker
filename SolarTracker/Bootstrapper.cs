using aemarcoCommons.ToolboxAppOptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SolarTracker.Services;

namespace SolarTracker;

public static class Bootstrapper
{

    public static WebApplicationBuilder SetupServices(this WebApplicationBuilder builder)
    {
        var sc = builder.Services;
        sc.AddConfigOptionsUtils(builder.Configuration, _ =>
        {

        });

        //sc.SetupToolbox();

        sc.AddHostedService<MainService>();
        sc.AddSingleton<TargetTrackerService>();
        sc.AddTransient<AstroApiService>();

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
