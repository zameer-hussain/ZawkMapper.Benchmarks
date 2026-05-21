using Microsoft.EntityFrameworkCore;
using ZawkMapper.Benchmarks.Configuration;
using ZawkMapper.Benchmarks.Models;

namespace ZawkMapper.Benchmarks.Data;

public static class DatabaseSeeder
{
    private static readonly string[] FirstNames = ["Ali", "Sara", "Ahmed", "Ayesha", "Bilal", "Hina", "Usman", "Fatima", "Danish", "Iqra"];
    private static readonly string[] LastNames = ["Mirza", "Vighio", "Khan", "Shaikh", "Memon", "Abbasi", "Soomro", "Rajput", "Malik", "Qureshi"];
    private static readonly string[] Cities = ["Karachi", "Hyderabad", "Lahore", "Islamabad", "Peshawar", "Quetta", "Sukkur", "Multan"];
    private static readonly string[] States = ["Sindh", "Punjab", "KPK", "Balochistan", "ICT"];
    private static readonly string[] Categories = ["Lab Equipment", "Medical", "Books", "Software", "Hardware", "Office", "Electronics"];
    private static readonly string[] Statuses = ["Pending", "Paid", "Processing", "Completed", "Cancelled"];
    private static readonly string[] PaymentMethods = ["Card", "Bank Transfer", "Wallet", "Cash", "Invoice"];

    public static async Task EnsureCreatedAndSeededAsync(BenchmarkRunOptions options, CancellationToken cancellationToken = default)
    {
        await using var db = DbContextFactory.Create(options.DatabasePath);
        await db.Database.EnsureCreatedAsync(cancellationToken);

        if (await db.Customers.AnyAsync(cancellationToken))
        {
            Console.WriteLine("Database already contains data. Seeding skipped.");
            Console.WriteLine($"Database: {options.DatabasePath}");
            return;
        }

        Console.WriteLine("Seeding database. This can take a few minutes for large data sizes...");
        await SeedProductsAsync(db, options.Products, cancellationToken);
        await SeedCustomersAsync(db, options.Customers, cancellationToken);
        await SeedOrdersAsync(db, options.Orders, options.OrderItems, options.Customers, options.Products, cancellationToken);
        Console.WriteLine("Seeding completed.");
    }

    private static async Task SeedProductsAsync(AppDbContext db, int count, CancellationToken cancellationToken)
    {
        var products = new List<Product>(count);
        for (var i = 1; i <= count; i++)
        {
            products.Add(new Product
            {
                Sku = $"SKU-{i:000000}",
                Name = $"Product {i:000000}",
                Category = Categories[i % Categories.Length],
                Price = 100 + (i % 9000)
            });
        }
        db.Products.AddRange(products);
        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedCustomersAsync(AppDbContext db, int count, CancellationToken cancellationToken)
    {
        const int batchSize = 5000;
        for (var start = 1; start <= count; start += batchSize)
        {
            var batch = new List<Customer>(Math.Min(batchSize, count - start + 1));
            for (var i = start; i < start + batchSize && i <= count; i++)
            {
                var first = FirstNames[i % FirstNames.Length];
                var last = LastNames[i % LastNames.Length];
                batch.Add(new Customer
                {
                    CustomerCode = $"CUS-{i:000000}",
                    FirstName = first,
                    LastName = last,
                    Email = $"{first.ToLowerInvariant()}.{last.ToLowerInvariant()}.{i}@example.com",
                    Phone = $"+92-300-{i % 9999999:0000000}",
                    CreatedUtc = DateTime.UtcNow.AddDays(-(i % 3650)),
                    Address = new Address
                    {
                        City = Cities[i % Cities.Length],
                        State = States[i % States.Length],
                        Country = "Pakistan",
                        PostalCode = $"{70000 + (i % 9999)}"
                    }
                });
            }

            db.Customers.AddRange(batch);
            await db.SaveChangesAsync(cancellationToken);
            Console.WriteLine($"Seeded customers: {Math.Min(start + batchSize - 1, count):N0}/{count:N0}");
        }
    }

    private static async Task SeedOrdersAsync(AppDbContext db, int ordersCount, int itemsCount, int customerCount, int productCount, CancellationToken cancellationToken)
    {
        const int orderBatchSize = 5000;
        for (var start = 1; start <= ordersCount; start += orderBatchSize)
        {
            var batch = new List<Order>(Math.Min(orderBatchSize, ordersCount - start + 1));
            for (var i = start; i < start + orderBatchSize && i <= ordersCount; i++)
            {
                var order = new Order
                {
                    OrderNumber = $"ORD-{i:000000}",
                    CustomerId = ((i - 1) % customerCount) + 1,
                    OrderDateUtc = DateTime.UtcNow.AddDays(-(i % 730)),
                    Status = Statuses[i % Statuses.Length]
                };

                batch.Add(order);
            }

            db.Orders.AddRange(batch);
            await db.SaveChangesAsync(cancellationToken);
            Console.WriteLine($"Seeded orders: {Math.Min(start + orderBatchSize - 1, ordersCount):N0}/{ordersCount:N0}");
        }

        const int itemBatchSize = 10000;
        for (var start = 1; start <= itemsCount; start += itemBatchSize)
        {
            var items = new List<OrderItem>(Math.Min(itemBatchSize, itemsCount - start + 1));
            for (var i = start; i < start + itemBatchSize && i <= itemsCount; i++)
            {
                items.Add(new OrderItem
                {
                    OrderId = ((i - 1) % ordersCount) + 1,
                    ProductId = ((i - 1) % productCount) + 1,
                    Quantity = (i % 5) + 1,
                    UnitPrice = 100 + (i % 9000)
                });
            }
            db.OrderItems.AddRange(items);
            await db.SaveChangesAsync(cancellationToken);
            Console.WriteLine($"Seeded order items: {Math.Min(start + itemBatchSize - 1, itemsCount):N0}/{itemsCount:N0}");
        }

        const int paymentBatchSize = 5000;
        for (var start = 1; start <= ordersCount; start += paymentBatchSize)
        {
            var payments = new List<Payment>(Math.Min(paymentBatchSize, ordersCount - start + 1));
            for (var i = start; i < start + paymentBatchSize && i <= ordersCount; i++)
            {
                payments.Add(new Payment
                {
                    OrderId = i,
                    Amount = 500 + (i % 100000),
                    Method = PaymentMethods[i % PaymentMethods.Length],
                    ProviderReference = $"PAY-{i:000000}",
                    PaidUtc = DateTime.UtcNow.AddDays(-(i % 730))
                });
            }
            db.Payments.AddRange(payments);
            await db.SaveChangesAsync(cancellationToken);
            Console.WriteLine($"Seeded payments: {Math.Min(start + paymentBatchSize - 1, ordersCount):N0}/{ordersCount:N0}");
        }
    }
}
