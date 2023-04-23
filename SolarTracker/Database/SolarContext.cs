using Microsoft.EntityFrameworkCore;

namespace SolarTracker.Database;

public class SolarContext : DbContext
{
    public SolarContext(DbContextOptions<SolarContext> options)
        : base(options)
    { }

    public DbSet<KeyValueInfo> KeyValueInfos { get; set; } = null!;
    public DbSet<SunInfo> SunInfos { get; set; } = null!;


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<KeyValueInfo>(e =>
        {
            e.ToTable("KeyValueInfos");
            e.HasKey(x => x.Key);
            e.Property<string>("Key")
                .HasColumnType("TEXT")
                .HasMaxLength(100)
                .ValueGeneratedNever();
        });

        modelBuilder.Entity<SunInfo>(e =>
        {
            e.ToTable("SolarInfos");
            e.HasKey(x => new { x.Timestamp, x.Latitude, x.Longitude });

        });
    }
}