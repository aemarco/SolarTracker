using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;

namespace SolarTracker.Database;

//ioc
public static class SolarContextFactoryExtensions
{
    public static IServiceCollection SetupDatabase(this IServiceCollection sc)
    {
        sc.AddTransient(sp =>
        {
            var dbOptionsBuilder = new DbContextOptionsBuilder<SolarContext>()
                .UseSqlite("Data Source=app.db");
            var loggerFactory = sp.GetService<ILoggerFactory>();
            if (loggerFactory is not null)
            {
                dbOptionsBuilder.UseLoggerFactory(loggerFactory);
            }
            return dbOptionsBuilder.Options;
        });
        sc.AddSingleton<SolarContextFactory>();

        return sc;
    }
}


//run time
public class SolarContextFactory
{
    private readonly DbContextOptions<SolarContext> _options;
    private bool _checkMigration = true;
    private readonly object _lock = new();

    public SolarContextFactory(
        DbContextOptions<SolarContext> options)
    {
        _options = options;
    }

    public SolarContext Create()
    {
        var context = new SolarContext(_options);

        if (!_checkMigration)
            return context;

        lock (_lock)
        {
            if (!_checkMigration)
                return context;

            context.Database.Migrate();

            _checkMigration = false;
            return context;
        }
    }
}


//design time
public class SolarContextDesignTimeFactory : IDesignTimeDbContextFactory<SolarContext>
{
    public SolarContext CreateDbContext(string[] args)
    {
        var sp = new ServiceCollection()
            .SetupDatabase()
            .BuildServiceProvider();
        var options = sp.GetRequiredService<DbContextOptions<SolarContext>>();
        var context = new SolarContext(options);
        return context;
    }
}
