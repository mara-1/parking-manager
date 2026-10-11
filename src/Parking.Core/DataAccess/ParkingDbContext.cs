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

    public DbSet<ParkingSpot> ParkingSpots => Set<ParkingSpot>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.Entity<OperatorAccount>().HasIndex(o => o.Login).IsUnique();
        modelBuilder.Entity<ParkingSpot>().HasIndex(s => s.Number).IsUnique();
        modelBuilder.Entity<ParkingSpot>().Property(s => s.Status).HasConversion<string>();
    }
}
