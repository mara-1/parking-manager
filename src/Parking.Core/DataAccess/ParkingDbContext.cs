using Microsoft.EntityFrameworkCore;
using Parking.Core.Models;

namespace Parking.Core.DataAccess;

public class ParkingDbContext : DbContext
{
    public ParkingDbContext(DbContextOptions<ParkingDbContext> options)
        : base(options)
    {
    }

    public DbSet<OperatorAccount> Operators => Set<OperatorAccount>();

    public DbSet<CategoryCapacity> Categories => Set<CategoryCapacity>();

    public DbSet<ParkingSession> Sessions => Set<ParkingSession>();

    public DbSet<Tariff> Tariffs => Set<Tariff>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.Entity<OperatorAccount>().HasIndex(o => o.Login).IsUnique();

        modelBuilder.Entity<CategoryCapacity>().HasKey(c => c.Category);
        modelBuilder.Entity<CategoryCapacity>().Property(c => c.Category).HasConversion<string>();

        modelBuilder.Entity<Tariff>().HasKey(t => t.Category);
        modelBuilder.Entity<Tariff>().Property(t => t.Category).HasConversion<string>();

        var session = modelBuilder.Entity<ParkingSession>();
        session.Property(s => s.Category).HasConversion<string>();
        session.Property(s => s.CarNumber).HasMaxLength(16);
        session.Ignore(s => s.IsActive);
        session.HasIndex(s => s.CarNumber);
        session.HasIndex(s => s.ExitTime);
        session.HasIndex(s => s.EntryTime);
    }
}
