using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using ZawkMapper.Benchmarks.Configuration;
using ZawkMapper.Benchmarks.Data;
using ZawkMapper.Benchmarks.Dtos;
using ZawkMapper.Benchmarks.Mapping;
using ZawkMapper.Benchmarks.Models;
using ZawkMapper.Benchmarks.Reporting;

namespace ZawkMapper.Benchmarks;

public static class ComparisonJob
{
    public static async Task RunAsync(BenchmarkRunOptions options, CancellationToken cancellationToken = default)
    {
        await DatabaseSeeder.EnsureCreatedAndSeededAsync(options, cancellationToken);

        await using var db = DbContextFactory.Create(options.DatabasePath);
        var take = Math.Max(1, options.Take);
        var orderTake = Math.Min(take, 50_000);

        Console.WriteLine($"Loading customers for runtime mapping: {take:N0}");
        var customers = await db.Customers
            .AsNoTracking()
            .Include(x => x.Address)
            .OrderBy(x => x.Id)
            .Take(take)
            .ToListAsync(cancellationToken);

        Console.WriteLine($"Loading orders for runtime mapping: {orderTake:N0}");
        var orders = await db.Orders
            .AsNoTracking()
            .Include(x => x.Customer).ThenInclude(x => x.Address)
            .Include(x => x.Items).ThenInclude(x => x.Product)
            .OrderBy(x => x.Id)
            .Take(orderTake)
            .ToListAsync(cancellationToken);

        var autoMapper = AutoMapperFactory.Create();
        var zawkMappers = new[]
        {
            new ZawkMapperRunner(ZawkRuntimeMapStyle.FlexibleMapField),
            new ZawkMapperRunner(ZawkRuntimeMapStyle.DirectMapFieldDirect),
            new ZawkMapperRunner(ZawkRuntimeMapStyle.StrictMapFieldStrict),
            new ZawkMapperRunner(ZawkRuntimeMapStyle.MixedCurrentStyle)
        };
        var projectionRunner = zawkMappers.Single(x => x.RuntimeMapStyle == ZawkRuntimeMapStyle.MixedCurrentStyle);
        var metrics = new List<ComparisonMetric>();

        AddRuntimeFlatCustomerMetrics(metrics, customers, autoMapper, zawkMappers);
        AddRuntimeOrderSummaryMetrics(metrics, orders, autoMapper, zawkMappers);
        AddRuntimeOrderDetailMetrics(metrics, orders, autoMapper, zawkMappers);
        AddRuntimeFullWorkloadMetrics(metrics, customers, orders, autoMapper, zawkMappers);
        AddProjectionMetrics(metrics, db, take, orderTake, autoMapper, projectionRunner, cancellationToken);

        ExcelReportWriter.Write(metrics, options.ResultsDirectory);
        PrintSummary(metrics, options.ResultsDirectory);
    }//RunAsync

    private static void AddRuntimeFlatCustomerMetrics(
        ICollection<ComparisonMetric> metrics,
        IReadOnlyList<Customer> customers,
        IMapper autoMapper,
        IReadOnlyList<ZawkMapperRunner> zawkMappers)
    {
        const string scenario = "Runtime flat customer DTO";
        metrics.Add(Measurement.Run(scenario, "Manual Mapping", customers.Count, () => ManualMapper.ToCustomerFlatDtos(customers)));
        metrics.Add(Measurement.Run(scenario, "AutoMapper", customers.Count, () => autoMapper.Map<List<CustomerFlatDto>>(customers)));

        foreach (var runner in zawkMappers)
        {
            metrics.Add(Measurement.Run(scenario, runner.DisplayName, customers.Count, () => runner.MapCustomers(customers)));
        }//foreach
    }//AddRuntimeFlatCustomerMetrics

    private static void AddRuntimeOrderSummaryMetrics(
        ICollection<ComparisonMetric> metrics,
        IReadOnlyList<Order> orders,
        IMapper autoMapper,
        IReadOnlyList<ZawkMapperRunner> zawkMappers)
    {
        const string scenario = "Runtime order summary DTO";
        metrics.Add(Measurement.Run(scenario, "Manual Mapping", orders.Count, () => ManualMapper.ToOrderSummaryDtos(orders)));
        metrics.Add(Measurement.Run(scenario, "AutoMapper", orders.Count, () => autoMapper.Map<List<OrderSummaryDto>>(orders)));

        foreach (var runner in zawkMappers)
        {
            metrics.Add(Measurement.Run(scenario, runner.DisplayName, orders.Count, () => runner.MapOrderSummaries(orders)));
        }//foreach
    }//AddRuntimeOrderSummaryMetrics

    private static void AddRuntimeOrderDetailMetrics(
        ICollection<ComparisonMetric> metrics,
        IReadOnlyList<Order> orders,
        IMapper autoMapper,
        IReadOnlyList<ZawkMapperRunner> zawkMappers)
    {
        const string scenario = "Runtime nested order detail DTO";
        metrics.Add(Measurement.Run(scenario, "Manual Mapping", orders.Count, () => ManualMapper.ToOrderDetailDtos(orders)));
        metrics.Add(Measurement.Run(scenario, "AutoMapper", orders.Count, () => autoMapper.Map<List<OrderDetailDto>>(orders)));

        foreach (var runner in zawkMappers)
        {
            metrics.Add(Measurement.Run(scenario, runner.DisplayName, orders.Count, () => runner.MapOrderDetails(orders)));
        }//foreach
    }//AddRuntimeOrderDetailMetrics

    private static void AddRuntimeFullWorkloadMetrics(
        ICollection<ComparisonMetric> metrics,
        IReadOnlyList<Customer> customers,
        IReadOnlyList<Order> orders,
        IMapper autoMapper,
        IReadOnlyList<ZawkMapperRunner> zawkMappers)
    {
        const string scenario = "Runtime cumulative flat + summary + nested";
        var records = customers.Count + orders.Count + orders.Count;
        metrics.Add(Measurement.RunValue(scenario, "Manual Mapping", records, () => ManualMapper.ToFullRuntimeWorkload(customers, orders)));
        metrics.Add(Measurement.RunValue(scenario, "AutoMapper", records, () => new RuntimeMappingWorkloadResult(
            autoMapper.Map<List<CustomerFlatDto>>(customers),
            autoMapper.Map<List<OrderSummaryDto>>(orders),
            autoMapper.Map<List<OrderDetailDto>>(orders))));

        foreach (var runner in zawkMappers)
        {
            metrics.Add(Measurement.RunValue(scenario, runner.DisplayName, records, () => runner.MapFullRuntimeWorkload(customers, orders)));
        }//foreach
    }//AddRuntimeFullWorkloadMetrics

    private static void AddProjectionMetrics(
        ICollection<ComparisonMetric> metrics,
        AppDbContext db,
        int take,
        int orderTake,
        IMapper autoMapper,
        ZawkMapperRunner zawkMapper,
        CancellationToken cancellationToken)
    {
        metrics.Add(Measurement.Run("EF projection flat customer DTO", "Manual Select", take, () => ProjectManualCustomers(db, take, cancellationToken)));
        metrics.Add(Measurement.Run("EF projection flat customer DTO", "AutoMapper ProjectTo", take, () => db.Customers
            .AsNoTracking()
            .OrderBy(x => x.Id)
            .Take(take)
            .ProjectTo<CustomerFlatDto>(autoMapper.ConfigurationProvider)
            .ToListAsync(cancellationToken)
            .GetAwaiter()
            .GetResult()));
        metrics.Add(Measurement.Run("EF projection flat customer DTO", "ZawkMapper ProjectAs", take, () => zawkMapper
            .ProjectCustomersAsync(db.Customers, take, cancellationToken)
            .GetAwaiter()
            .GetResult()));

        metrics.Add(Measurement.Run("EF projection order summary DTO", "Manual Select", orderTake, () => ProjectManualOrderSummaries(db, orderTake, cancellationToken)));
        metrics.Add(Measurement.Run("EF projection order summary DTO", "AutoMapper ProjectTo", orderTake, () => db.Orders
            .AsNoTracking()
            .OrderBy(x => x.Id)
            .Take(orderTake)
            .ProjectTo<OrderSummaryDto>(autoMapper.ConfigurationProvider)
            .ToListAsync(cancellationToken)
            .GetAwaiter()
            .GetResult()));
        metrics.Add(Measurement.Run("EF projection order summary DTO", "ZawkMapper ProjectAs", orderTake, () => zawkMapper
            .ProjectOrderSummariesAsync(db.Orders, orderTake, cancellationToken)
            .GetAwaiter()
            .GetResult()));

        var detailProjectionTake = Math.Min(orderTake, 10_000);
        metrics.Add(Measurement.Run("EF projection nested order detail DTO", "Manual Select", detailProjectionTake, () => ProjectManualOrderDetails(db, detailProjectionTake, cancellationToken)));
        metrics.Add(Measurement.Run("EF projection nested order detail DTO", "AutoMapper ProjectTo", detailProjectionTake, () => db.Orders
            .AsNoTracking()
            .OrderBy(x => x.Id)
            .Take(detailProjectionTake)
            .ProjectTo<OrderDetailDto>(autoMapper.ConfigurationProvider)
            .ToListAsync(cancellationToken)
            .GetAwaiter()
            .GetResult()));
        metrics.Add(Measurement.Run("EF projection nested order detail DTO", "ZawkMapper ProjectAs", detailProjectionTake, () => zawkMapper
            .ProjectOrderDetailsAsync(db.Orders, detailProjectionTake, cancellationToken)
            .GetAwaiter()
            .GetResult()));
    }//AddProjectionMetrics

    private static List<CustomerFlatDto> ProjectManualCustomers(AppDbContext db, int take, CancellationToken cancellationToken)
    {
        return db.Customers
            .AsNoTracking()
            .OrderBy(x => x.Id)
            .Take(take)
            .Select(x => new CustomerFlatDto
            {
                Id = x.Id,
                CustomerCode = x.CustomerCode,
                FullName = x.FirstName + " " + x.LastName,
                Email = x.Email,
                City = x.Address == null ? string.Empty : x.Address.City,
                Country = x.Address == null ? string.Empty : x.Address.Country
            })
            .ToListAsync(cancellationToken)
            .GetAwaiter()
            .GetResult();
    }//ProjectManualCustomers

    private static List<OrderSummaryDto> ProjectManualOrderSummaries(AppDbContext db, int take, CancellationToken cancellationToken)
    {
        return db.Orders
            .AsNoTracking()
            .OrderBy(x => x.Id)
            .Take(take)
            .Select(x => new OrderSummaryDto
            {
                Id = x.Id,
                OrderNumber = x.OrderNumber,
                CustomerName = x.Customer.FirstName + " " + x.Customer.LastName,
                ItemsCount = x.Items.Count(),
                Total = x.Items.Sum(i => i.Quantity * (double)i.UnitPrice),
                Status = x.Status,
                OrderDateUtc = x.OrderDateUtc
            })
            .ToListAsync(cancellationToken)
            .GetAwaiter()
            .GetResult();
    }//ProjectManualOrderSummaries

    private static List<OrderDetailDto> ProjectManualOrderDetails(AppDbContext db, int take, CancellationToken cancellationToken)
    {
        return db.Orders
            .AsNoTracking()
            .OrderBy(x => x.Id)
            .Take(take)
            .Select(x => new OrderDetailDto
            {
                Id = x.Id,
                OrderNumber = x.OrderNumber,
                CustomerName = x.Customer.FirstName + " " + x.Customer.LastName,
                City = x.Customer.Address == null ? string.Empty : x.Customer.Address.City,
                Country = x.Customer.Address == null ? string.Empty : x.Customer.Address.Country,
                Total = x.Items.Sum(i => i.Quantity * (double)i.UnitPrice),
                Lines = x.Items.Select(i => new OrderLineDto
                {
                    ProductId = i.ProductId,
                    ProductName = i.Product.Name,
                    Category = i.Product.Category,
                    Quantity = i.Quantity,
                    UnitPrice = (double)i.UnitPrice,
                    LineTotal = i.Quantity * (double)i.UnitPrice
                }).ToList()
            })
            .ToListAsync(cancellationToken)
            .GetAwaiter()
            .GetResult();
    }//ProjectManualOrderDetails

    private static void PrintSummary(IReadOnlyList<ComparisonMetric> metrics, string resultsDirectory)
    {
        Console.WriteLine();
        Console.WriteLine("Results");
        Console.WriteLine("-------");
        foreach (var group in metrics.GroupBy(x => x.Scenario))
        {
            Console.WriteLine(group.Key);
            foreach (var item in group.OrderBy(x => x.ElapsedMilliseconds))
            {
                Console.WriteLine($"  {item.Mapper,-28} {item.ElapsedMilliseconds,10:0.000} ms  {item.AllocatedMegabytes,10:0.00} MB  {item.MicrosecondsPerRecord,8:0.000} µs/record  {item.BytesPerRecord,9:0.00} B/record  {item.Checksum}");
            }//foreach
        }//foreach

        Console.WriteLine();
        Console.WriteLine("ZawkMapper runtime variants:");
        Console.WriteLine("  ZawkMapper MapField       = every runtime member configured with MapField.");
        Console.WriteLine("  ZawkMapper MapFieldDirect = direct assignment where valid; nested collection bridge still uses MapField.");
        Console.WriteLine("  ZawkMapper MapFieldStrict = strict same-type member mapping where valid; nested collection bridge still uses MapField.");
        Console.WriteLine("  ZawkMapper Mixed          = current realistic style: direct for simple fields, flexible for computed/converted/nested fields.");
        Console.WriteLine();
        Console.WriteLine($"Excel: {Path.Combine(resultsDirectory, "MapperComparison.xlsx")}");
        Console.WriteLine($"CSV:   {Path.Combine(resultsDirectory, "MapperComparison.csv")}");
    }//PrintSummary
}
