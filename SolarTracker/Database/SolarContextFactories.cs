using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;

namespace SolarTracker.Database;


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
