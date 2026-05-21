using Microsoft.EntityFrameworkCore;
using ZawkMapper.Benchmarks.Models;

namespace ZawkMapper.Benchmarks.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasIndex(x => x.CustomerCode).IsUnique();
            entity.HasOne(x => x.Address).WithOne(x => x.Customer).HasForeignKey<Address>(x => x.CustomerId);
            entity.Property(x => x.FirstName).HasMaxLength(80);
            entity.Property(x => x.LastName).HasMaxLength(80);
            entity.Property(x => x.Email).HasMaxLength(160);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasIndex(x => x.Sku).IsUnique();
            entity.Property(x => x.Price).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasIndex(x => x.OrderNumber).IsUnique();
            entity.HasMany(x => x.Items).WithOne(x => x.Order).HasForeignKey(x => x.OrderId);
            entity.HasOne(x => x.Payment).WithOne(x => x.Order).HasForeignKey<Payment>(x => x.OrderId);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.Property(x => x.UnitPrice).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.Property(x => x.Amount).HasPrecision(18, 2);
        });
    }
}

public static class DbContextFactory
{
    public static AppDbContext Create(string databasePath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(databasePath)!);
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source={databasePath}")
            .EnableThreadSafetyChecks(false)
            .Options;

        return new AppDbContext(options);
    }
}
