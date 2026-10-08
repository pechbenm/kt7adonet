using KT7.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace KT7.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Student> Students => Set<Student>();
    public DbSet<TaskItem> TaskItems => Set<TaskItem>();
    public DbSet<Resource> Resources => Set<Resource>();
    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void ConfigureConventions(ModelConfigurationBuilder builder)
        => builder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();

    protected override void OnModelCreating(ModelBuilder m)
    {
        m.Entity<Student>().HasIndex(s => s.Email).IsUnique();

        m.Entity<TaskItem>().Property(t => t.Status).HasConversion<string>().HasMaxLength(20);
        m.Entity<TaskItem>().HasIndex(t => t.Status);

        m.Entity<Booking>().Property(b => b.Status).HasConversion<string>().HasMaxLength(20);
        m.Entity<Booking>().HasIndex(b => new { b.ResourceId, b.Start, b.End });

        m.Entity<Resource>().HasData(
            new Resource { Id = 1, Type = "room", Name = "Номер 101", Capacity = 2 },
            new Resource { Id = 2, Type = "room", Name = "Номер 102", Capacity = 3 },
            new Resource { Id = 3, Type = "room", Name = "Люкс 201", Capacity = 4 },
            new Resource { Id = 4, Type = "table", Name = "Столик у окна", Capacity = 2 },
            new Resource { Id = 5, Type = "table", Name = "Столик 5", Capacity = 6 });
    }
}

public class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
{
    public UtcDateTimeConverter() : base(
        v => v.ToUniversalTime(),
        v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
    { }
}