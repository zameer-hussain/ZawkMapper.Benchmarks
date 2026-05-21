using AutoMapper;
using AutoMapper.QueryableExtensions;
using BenchmarkDotNet.Attributes;
using Microsoft.EntityFrameworkCore;
using ZawkMapper.Benchmarks.Configuration;
using ZawkMapper.Benchmarks.Data;
using ZawkMapper.Benchmarks.Dtos;
using ZawkMapper.Benchmarks.Mapping;
using ZawkMapper.Benchmarks.Models;

namespace ZawkMapper.Benchmarks.Benchmarks;

[MemoryDiagnoser]
[SimpleJob]
public class MapperBenchmarks
{
    private AppDbContext _db = null!;
    private List<Customer> _customers = new();
    private List<Order> _orders = new();
    private IMapper _autoMapper = null!;
    private ZawkMapperRunner _zawkMapField = null!;
    private ZawkMapperRunner _zawkMapFieldDirect = null!;
    private ZawkMapperRunner _zawkMapFieldStrict = null!;
    private ZawkMapperRunner _zawkMixed = null!;

    [Params(1000, 10000)]
    public int Count { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var options = BenchmarkRunOptions.Parse([]);
        DatabaseSeeder.EnsureCreatedAndSeededAsync(options).GetAwaiter().GetResult();

        _db = DbContextFactory.Create(options.DatabasePath);
        _customers = _db.Customers
            .AsNoTracking()
            .Include(x => x.Address)
            .OrderBy(x => x.Id)
            .Take(Count)
            .ToList();

        _orders = _db.Orders
            .AsNoTracking()
            .Include(x => x.Customer).ThenInclude(x => x.Address)
            .Include(x => x.Items).ThenInclude(x => x.Product)
            .OrderBy(x => x.Id)
            .Take(Math.Min(Count, 5000))
            .ToList();

        _autoMapper = AutoMapperFactory.Create();
        _zawkMapField = new ZawkMapperRunner(ZawkRuntimeMapStyle.FlexibleMapField);
        _zawkMapFieldDirect = new ZawkMapperRunner(ZawkRuntimeMapStyle.DirectMapFieldDirect);
        _zawkMapFieldStrict = new ZawkMapperRunner(ZawkRuntimeMapStyle.StrictMapFieldStrict);
        _zawkMixed = new ZawkMapperRunner(ZawkRuntimeMapStyle.MixedCurrentStyle);
    }//Setup

    [GlobalCleanup]
    public void Cleanup()
    {
        _db.Dispose();
    }//Cleanup

    [Benchmark(Baseline = true)]
    public List<CustomerFlatDto> Manual_FlatCustomerDto() => ManualMapper.ToCustomerFlatDtos(_customers);

    [Benchmark]
    public List<CustomerFlatDto> AutoMapper_FlatCustomerDto() => _autoMapper.Map<List<CustomerFlatDto>>(_customers);

    [Benchmark]
    public List<CustomerFlatDto> ZawkMapper_MapField_FlatCustomerDto() => _zawkMapField.MapCustomers(_customers);

    [Benchmark]
    public List<CustomerFlatDto> ZawkMapper_MapFieldDirect_FlatCustomerDto() => _zawkMapFieldDirect.MapCustomers(_customers);

    [Benchmark]
    public List<CustomerFlatDto> ZawkMapper_MapFieldStrict_FlatCustomerDto() => _zawkMapFieldStrict.MapCustomers(_customers);

    [Benchmark]
    public List<CustomerFlatDto> ZawkMapper_Mixed_FlatCustomerDto() => _zawkMixed.MapCustomers(_customers);

    [Benchmark]
    public List<OrderSummaryDto> Manual_OrderSummaryDto() => ManualMapper.ToOrderSummaryDtos(_orders);

    [Benchmark]
    public List<OrderSummaryDto> AutoMapper_OrderSummaryDto() => _autoMapper.Map<List<OrderSummaryDto>>(_orders);

    [Benchmark]
    public List<OrderSummaryDto> ZawkMapper_MapField_OrderSummaryDto() => _zawkMapField.MapOrderSummaries(_orders);

    [Benchmark]
    public List<OrderSummaryDto> ZawkMapper_MapFieldDirect_OrderSummaryDto() => _zawkMapFieldDirect.MapOrderSummaries(_orders);

    [Benchmark]
    public List<OrderSummaryDto> ZawkMapper_MapFieldStrict_OrderSummaryDto() => _zawkMapFieldStrict.MapOrderSummaries(_orders);

    [Benchmark]
    public List<OrderSummaryDto> ZawkMapper_Mixed_OrderSummaryDto() => _zawkMixed.MapOrderSummaries(_orders);

    [Benchmark]
    public List<OrderDetailDto> Manual_OrderDetailDto() => ManualMapper.ToOrderDetailDtos(_orders);

    [Benchmark]
    public List<OrderDetailDto> AutoMapper_OrderDetailDto() => _autoMapper.Map<List<OrderDetailDto>>(_orders);

    [Benchmark]
    public List<OrderDetailDto> ZawkMapper_MapField_OrderDetailDto() => _zawkMapField.MapOrderDetails(_orders);

    [Benchmark]
    public List<OrderDetailDto> ZawkMapper_MapFieldDirect_OrderDetailDto() => _zawkMapFieldDirect.MapOrderDetails(_orders);

    [Benchmark]
    public List<OrderDetailDto> ZawkMapper_MapFieldStrict_OrderDetailDto() => _zawkMapFieldStrict.MapOrderDetails(_orders);

    [Benchmark]
    public List<OrderDetailDto> ZawkMapper_Mixed_OrderDetailDto() => _zawkMixed.MapOrderDetails(_orders);

    [Benchmark]
    public RuntimeMappingWorkloadResult Manual_CumulativeRuntimeWorkload() => ManualMapper.ToFullRuntimeWorkload(_customers, _orders);

    [Benchmark]
    public RuntimeMappingWorkloadResult AutoMapper_CumulativeRuntimeWorkload() => new(
        _autoMapper.Map<List<CustomerFlatDto>>(_customers),
        _autoMapper.Map<List<OrderSummaryDto>>(_orders),
        _autoMapper.Map<List<OrderDetailDto>>(_orders));

    [Benchmark]
    public RuntimeMappingWorkloadResult ZawkMapper_MapField_CumulativeRuntimeWorkload() => _zawkMapField.MapFullRuntimeWorkload(_customers, _orders);

    [Benchmark]
    public RuntimeMappingWorkloadResult ZawkMapper_MapFieldDirect_CumulativeRuntimeWorkload() => _zawkMapFieldDirect.MapFullRuntimeWorkload(_customers, _orders);

    [Benchmark]
    public RuntimeMappingWorkloadResult ZawkMapper_MapFieldStrict_CumulativeRuntimeWorkload() => _zawkMapFieldStrict.MapFullRuntimeWorkload(_customers, _orders);

    [Benchmark]
    public RuntimeMappingWorkloadResult ZawkMapper_Mixed_CumulativeRuntimeWorkload() => _zawkMixed.MapFullRuntimeWorkload(_customers, _orders);

    [Benchmark]
    public List<CustomerFlatDto> Manual_Project_FlatCustomerDto()
    {
        return _db.Customers
            .AsNoTracking()
            .OrderBy(x => x.Id)
            .Take(Count)
            .Select(x => new CustomerFlatDto
            {
                Id = x.Id,
                CustomerCode = x.CustomerCode,
                FullName = x.FirstName + " " + x.LastName,
                Email = x.Email,
                City = x.Address == null ? string.Empty : x.Address.City,
                Country = x.Address == null ? string.Empty : x.Address.Country
            })
            .ToList();
    }//Manual_Project_FlatCustomerDto

    [Benchmark]
    public List<CustomerFlatDto> AutoMapper_Project_FlatCustomerDto()
    {
        return _db.Customers
            .AsNoTracking()
            .OrderBy(x => x.Id)
            .Take(Count)
            .ProjectTo<CustomerFlatDto>(_autoMapper.ConfigurationProvider)
            .ToList();
    }//AutoMapper_Project_FlatCustomerDto

    [Benchmark]
    public List<CustomerFlatDto> ZawkMapper_Project_FlatCustomerDto()
    {
        return _zawkMixed.ProjectCustomersAsync(_db.Customers, Count)
            .GetAwaiter()
            .GetResult();
    }//ZawkMapper_Project_FlatCustomerDto

    [Benchmark]
    public List<OrderSummaryDto> Manual_Project_OrderSummaryDto()
    {
        var take = Math.Min(Count, 5000);
        return _db.Orders
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
            .ToList();
    }//Manual_Project_OrderSummaryDto

    [Benchmark]
    public List<OrderSummaryDto> AutoMapper_Project_OrderSummaryDto()
    {
        var take = Math.Min(Count, 5000);
        return _db.Orders
            .AsNoTracking()
            .OrderBy(x => x.Id)
            .Take(take)
            .ProjectTo<OrderSummaryDto>(_autoMapper.ConfigurationProvider)
            .ToList();
    }//AutoMapper_Project_OrderSummaryDto

    [Benchmark]
    public List<OrderSummaryDto> ZawkMapper_Project_OrderSummaryDto()
    {
        var take = Math.Min(Count, 5000);
        return _zawkMixed.ProjectOrderSummariesAsync(_db.Orders, take)
            .GetAwaiter()
            .GetResult();
    }//ZawkMapper_Project_OrderSummaryDto
}
