using Microsoft.EntityFrameworkCore;

namespace SolarTracker.Database;

public class SolarContext : DbContext
{
    public SolarContext(DbContextOptions<SolarContext> options)
        : base(options)
    { }

    public DbSet<KeyValueInfo> KeyValueInfos { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<KeyValueInfo>(b =>
        {
            b.Property<string>("Key")
                .HasColumnType("TEXT")
                .HasMaxLength(100)
                .ValueGeneratedNever();

            b.Property<string>("Value")
                .HasColumnType("TEXT");

            b.HasKey("Key");
            b.ToTable("KeyValueInfos");
        });
    }
}