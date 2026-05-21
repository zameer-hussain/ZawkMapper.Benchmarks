using Microsoft.EntityFrameworkCore;
using ZawkMapper.Benchmarks.Dtos;
using ZawkMapper.Benchmarks.Models;
using ZawkMapper.Configuration;
using ZawkMapper.Core;
using ZawkMapper.Extensions;

namespace ZawkMapper.Benchmarks.Mapping;

/// <summary>
/// Keeps the ZawkMapper setup in one place so the benchmark uses the public API the same way
/// a real application would use it: MapModel for runtime mapping and ProjectModel/ProjectAs
/// for SQL-friendly IQueryable projection.
/// </summary>
public sealed class ZawkMapperRunner
{
    private readonly MapperConfiguration _configuration;
    private readonly ObjectMapper _mapper;

    public ZawkMapperRunner(ZawkRuntimeMapStyle runtimeMapStyle = ZawkRuntimeMapStyle.MixedCurrentStyle)
    {
        RuntimeMapStyle = runtimeMapStyle;
        _configuration = new MapperConfiguration(Configure);
        _configuration.AssertConfigurationIsValid();
        _mapper = new ObjectMapper(_configuration);
    }//ZawkMapperRunner

    public ZawkRuntimeMapStyle RuntimeMapStyle { get; }

    public string DisplayName => RuntimeMapStyle switch
    {
        ZawkRuntimeMapStyle.FlexibleMapField => "ZawkMapper MapField",
        ZawkRuntimeMapStyle.DirectMapFieldDirect => "ZawkMapper MapFieldDirect",
        ZawkRuntimeMapStyle.StrictMapFieldStrict => "ZawkMapper MapFieldStrict",
        ZawkRuntimeMapStyle.MixedCurrentStyle => "ZawkMapper Mixed",
        _ => "ZawkMapper"
    };

    private void Configure(MapperConfiguration cfg)
    {
        RegisterRuntimeMaps(cfg, RuntimeMapStyle);
        RegisterProjectionMaps(cfg);
    }//Configure

    private static void RegisterRuntimeMaps(MapperConfiguration cfg, ZawkRuntimeMapStyle style)
    {
        switch (style)
        {
            case ZawkRuntimeMapStyle.FlexibleMapField:
                RegisterFlexibleRuntimeMaps(cfg);
                break;

            case ZawkRuntimeMapStyle.DirectMapFieldDirect:
                RegisterDirectRuntimeMaps(cfg);
                break;

            case ZawkRuntimeMapStyle.StrictMapFieldStrict:
                RegisterStrictRuntimeMaps(cfg);
                break;

            case ZawkRuntimeMapStyle.MixedCurrentStyle:
            default:
                RegisterMixedRuntimeMaps(cfg);
                break;
        }//switch
    }//RegisterRuntimeMaps

    private static void RegisterFlexibleRuntimeMaps(MapperConfiguration cfg)
    {
        cfg.MapModel<Customer, CustomerFlatDto>()
            .MapField(d => d.Id, s => s.Id)
            .MapField(d => d.CustomerCode, s => s.CustomerCode)
            .MapField(d => d.FullName, s => s.FirstName + " " + s.LastName)
            .MapField(d => d.Email, s => s.Email)
            .MapField(d => d.City, s => s.Address == null ? string.Empty : s.Address.City)
            .MapField(d => d.Country, s => s.Address == null ? string.Empty : s.Address.Country);

        cfg.MapModel<Order, OrderSummaryDto>()
            .MapField(d => d.Id, s => s.Id)
            .MapField(d => d.OrderNumber, s => s.OrderNumber)
            .MapField(d => d.CustomerName, s => s.Customer.FirstName + " " + s.Customer.LastName)
            .MapField(d => d.ItemsCount, s => s.Items.Count)
            .MapField(d => d.Total, s => s.Items.Sum(x => x.Quantity * (double)x.UnitPrice))
            .MapField(d => d.Status, s => s.Status)
            .MapField(d => d.OrderDateUtc, s => s.OrderDateUtc);

        cfg.MapModel<OrderItem, OrderLineDto>()
            .MapField(d => d.ProductId, s => s.ProductId)
            .MapField(d => d.ProductName, s => s.Product.Name)
            .MapField(d => d.Category, s => s.Product.Category)
            .MapField(d => d.Quantity, s => s.Quantity)
            .MapField(d => d.UnitPrice, s => (double)s.UnitPrice)
            .MapField(d => d.LineTotal, s => s.Quantity * (double)s.UnitPrice);

        cfg.MapModel<Order, OrderDetailDto>()
            .MapField(d => d.Id, s => s.Id)
            .MapField(d => d.OrderNumber, s => s.OrderNumber)
            .MapField(d => d.CustomerName, s => s.Customer.FirstName + " " + s.Customer.LastName)
            .MapField(d => d.City, s => s.Customer.Address == null ? string.Empty : s.Customer.Address.City)
            .MapField(d => d.Country, s => s.Customer.Address == null ? string.Empty : s.Customer.Address.Country)
            .MapField(d => d.Total, s => s.Items.Sum(x => x.Quantity * (double)x.UnitPrice))
            .MapField(d => d.Lines, s => s.Items);
    }//RegisterFlexibleRuntimeMaps

    private static void RegisterDirectRuntimeMaps(MapperConfiguration cfg)
    {
        cfg.MapModel<Customer, CustomerFlatDto>()
            .MapFieldDirect(d => d.Id, s => s.Id)
            .MapFieldDirect(d => d.CustomerCode, s => s.CustomerCode)
            .MapFieldDirect(d => d.FullName, s => s.FirstName + " " + s.LastName)
            .MapFieldDirect(d => d.Email, s => s.Email)
            .MapFieldDirect(d => d.City, s => s.Address == null ? string.Empty : s.Address.City)
            .MapFieldDirect(d => d.Country, s => s.Address == null ? string.Empty : s.Address.Country);

        cfg.MapModel<Order, OrderSummaryDto>()
            .MapFieldDirect(d => d.Id, s => s.Id)
            .MapFieldDirect(d => d.OrderNumber, s => s.OrderNumber)
            .MapFieldDirect(d => d.CustomerName, s => s.Customer.FirstName + " " + s.Customer.LastName)
            .MapFieldDirect(d => d.ItemsCount, s => s.Items.Count)
            .MapFieldDirect(d => d.Total, s => s.Items.Sum(x => x.Quantity * (double)x.UnitPrice))
            .MapFieldDirect(d => d.Status, s => s.Status)
            .MapFieldDirect(d => d.OrderDateUtc, s => s.OrderDateUtc);

        cfg.MapModel<OrderItem, OrderLineDto>()
            .MapFieldDirect(d => d.ProductId, s => s.ProductId)
            .MapFieldDirect(d => d.ProductName, s => s.Product.Name)
            .MapFieldDirect(d => d.Category, s => s.Product.Category)
            .MapFieldDirect(d => d.Quantity, s => s.Quantity)
            .MapFieldDirect(d => d.UnitPrice, s => (double)s.UnitPrice)
            .MapFieldDirect(d => d.LineTotal, s => s.Quantity * (double)s.UnitPrice);

        cfg.MapModel<Order, OrderDetailDto>()
            .MapFieldDirect(d => d.Id, s => s.Id)
            .MapFieldDirect(d => d.OrderNumber, s => s.OrderNumber)
            .MapFieldDirect(d => d.CustomerName, s => s.Customer.FirstName + " " + s.Customer.LastName)
            .MapFieldDirect(d => d.City, s => s.Customer.Address == null ? string.Empty : s.Customer.Address.City)
            .MapFieldDirect(d => d.Country, s => s.Customer.Address == null ? string.Empty : s.Customer.Address.Country)
            .MapFieldDirect(d => d.Total, s => s.Items.Sum(x => x.Quantity * (double)x.UnitPrice))
            .MapField(d => d.Lines, s => s.Items);
    }//RegisterDirectRuntimeMaps

    private static void RegisterStrictRuntimeMaps(MapperConfiguration cfg)
    {
        cfg.MapModel<Customer, CustomerFlatDto>()
            .MapFieldStrict(d => d.Id, s => s.Id)
            .MapFieldStrict(d => d.CustomerCode, s => s.CustomerCode)
            .MapFieldStrict(d => d.FullName, s => s.FirstName + " " + s.LastName)
            .MapFieldStrict(d => d.Email, s => s.Email)
            .MapFieldStrict(d => d.City, s => s.Address == null ? string.Empty : s.Address.City)
            .MapFieldStrict(d => d.Country, s => s.Address == null ? string.Empty : s.Address.Country);

        cfg.MapModel<Order, OrderSummaryDto>()
            .MapFieldStrict(d => d.Id, s => s.Id)
            .MapFieldStrict(d => d.OrderNumber, s => s.OrderNumber)
            .MapFieldStrict(d => d.CustomerName, s => s.Customer.FirstName + " " + s.Customer.LastName)
            .MapFieldStrict(d => d.ItemsCount, s => s.Items.Count)
            .MapFieldStrict(d => d.Total, s => s.Items.Sum(x => x.Quantity * (double)x.UnitPrice))
            .MapFieldStrict(d => d.Status, s => s.Status)
            .MapFieldStrict(d => d.OrderDateUtc, s => s.OrderDateUtc);

        cfg.MapModel<OrderItem, OrderLineDto>()
            .MapFieldStrict(d => d.ProductId, s => s.ProductId)
            .MapFieldStrict(d => d.ProductName, s => s.Product.Name)
            .MapFieldStrict(d => d.Category, s => s.Product.Category)
            .MapFieldStrict(d => d.Quantity, s => s.Quantity)
            .MapFieldStrict(d => d.UnitPrice, s => (double)s.UnitPrice)
            .MapFieldStrict(d => d.LineTotal, s => s.Quantity * (double)s.UnitPrice);

        cfg.MapModel<Order, OrderDetailDto>()
            .MapFieldStrict(d => d.Id, s => s.Id)
            .MapFieldStrict(d => d.OrderNumber, s => s.OrderNumber)
            .MapFieldStrict(d => d.CustomerName, s => s.Customer.FirstName + " " + s.Customer.LastName)
            .MapFieldStrict(d => d.City, s => s.Customer.Address == null ? string.Empty : s.Customer.Address.City)
            .MapFieldStrict(d => d.Country, s => s.Customer.Address == null ? string.Empty : s.Customer.Address.Country)
            .MapFieldStrict(d => d.Total, s => s.Items.Sum(x => x.Quantity * (double)x.UnitPrice))
            .MapField(d => d.Lines, s => s.Items);
    }//RegisterStrictRuntimeMaps

    private static void RegisterMixedRuntimeMaps(MapperConfiguration cfg)
    {
        cfg.MapModel<Customer, CustomerFlatDto>()
            .MapFieldDirect(d => d.Id, s => s.Id)
            .MapFieldDirect(d => d.CustomerCode, s => s.CustomerCode)
            .MapField(d => d.FullName, s => s.FirstName + " " + s.LastName)
            .MapFieldDirect(d => d.Email, s => s.Email)
            .MapField(d => d.City, s => s.Address == null ? string.Empty : s.Address.City)
            .MapField(d => d.Country, s => s.Address == null ? string.Empty : s.Address.Country);

        cfg.MapModel<Order, OrderSummaryDto>()
            .MapFieldDirect(d => d.Id, s => s.Id)
            .MapFieldDirect(d => d.OrderNumber, s => s.OrderNumber)
            .MapField(d => d.CustomerName, s => s.Customer.FirstName + " " + s.Customer.LastName)
            .MapField(d => d.ItemsCount, s => s.Items.Count)
            .MapField(d => d.Total, s => s.Items.Sum(x => x.Quantity * (double)x.UnitPrice))
            .MapFieldDirect(d => d.Status, s => s.Status)
            .MapFieldDirect(d => d.OrderDateUtc, s => s.OrderDateUtc);

        cfg.MapModel<OrderItem, OrderLineDto>()
            .MapFieldDirect(d => d.ProductId, s => s.ProductId)
            .MapField(d => d.ProductName, s => s.Product.Name)
            .MapField(d => d.Category, s => s.Product.Category)
            .MapFieldDirect(d => d.Quantity, s => s.Quantity)
            .MapField(d => d.UnitPrice, s => (double)s.UnitPrice)
            .MapField(d => d.LineTotal, s => s.Quantity * (double)s.UnitPrice);

        cfg.MapModel<Order, OrderDetailDto>()
            .MapFieldDirect(d => d.Id, s => s.Id)
            .MapFieldDirect(d => d.OrderNumber, s => s.OrderNumber)
            .MapField(d => d.CustomerName, s => s.Customer.FirstName + " " + s.Customer.LastName)
            .MapField(d => d.City, s => s.Customer.Address == null ? string.Empty : s.Customer.Address.City)
            .MapField(d => d.Country, s => s.Customer.Address == null ? string.Empty : s.Customer.Address.Country)
            .MapField(d => d.Total, s => s.Items.Sum(x => x.Quantity * (double)x.UnitPrice))
            .MapField(d => d.Lines, s => s.Items);
    }//RegisterMixedRuntimeMaps

    private static void RegisterProjectionMaps(MapperConfiguration cfg)
    {
        cfg.ProjectModel<Customer, CustomerFlatDto>()
            .MapField(d => d.Id, s => s.Id)
            .MapField(d => d.CustomerCode, s => s.CustomerCode)
            .MapField(d => d.FullName, s => s.FirstName + " " + s.LastName)
            .MapField(d => d.Email, s => s.Email)
            .MapField(d => d.City, s => s.Address == null ? string.Empty : s.Address.City)
            .MapField(d => d.Country, s => s.Address == null ? string.Empty : s.Address.Country);

        cfg.ProjectModel<Order, OrderSummaryDto>()
            .MapField(d => d.Id, s => s.Id)
            .MapField(d => d.OrderNumber, s => s.OrderNumber)
            .MapField(d => d.CustomerName, s => s.Customer.FirstName + " " + s.Customer.LastName)
            .MapField(d => d.ItemsCount, s => s.Items.Count())
            .MapField(d => d.Total, s => s.Items.Sum(x => x.Quantity * (double)x.UnitPrice))
            .MapField(d => d.Status, s => s.Status)
            .MapField(d => d.OrderDateUtc, s => s.OrderDateUtc);

        cfg.ProjectModel<OrderItem, OrderLineDto>()
            .MapField(d => d.ProductId, s => s.ProductId)
            .MapField(d => d.ProductName, s => s.Product.Name)
            .MapField(d => d.Category, s => s.Product.Category)
            .MapField(d => d.Quantity, s => s.Quantity)
            .MapField(d => d.UnitPrice, s => s.UnitPrice)
            .MapField(d => d.LineTotal, s => s.Quantity * (double)s.UnitPrice);

        cfg.ProjectModel<Order, OrderDetailDto>()
            .MapField(d => d.Id, s => s.Id)
            .MapField(d => d.OrderNumber, s => s.OrderNumber)
            .MapField(d => d.CustomerName, s => s.Customer.FirstName + " " + s.Customer.LastName)
            .MapField(d => d.City, s => s.Customer.Address == null ? string.Empty : s.Customer.Address.City)
            .MapField(d => d.Country, s => s.Customer.Address == null ? string.Empty : s.Customer.Address.Country)
            .MapField(d => d.Total, s => s.Items.Sum(x => x.Quantity * (double)x.UnitPrice))
            .MapField(d => d.Lines, s => s.Items);
    }//RegisterProjectionMaps

    public List<CustomerFlatDto> MapCustomers(IReadOnlyList<Customer> customers)
    {
        var result = new List<CustomerFlatDto>(customers.Count);
        for (var i = 0; i < customers.Count; i++)
        {
            result.Add(_mapper.Map<Customer, CustomerFlatDto>(customers[i])!);
        }//for

        return result;
    }//MapCustomers

    public List<OrderSummaryDto> MapOrderSummaries(IReadOnlyList<Order> orders)
    {
        var result = new List<OrderSummaryDto>(orders.Count);
        for (var i = 0; i < orders.Count; i++)
        {
            result.Add(_mapper.Map<Order, OrderSummaryDto>(orders[i])!);
        }//for

        return result;
    }//MapOrderSummaries

    public List<OrderDetailDto> MapOrderDetails(IReadOnlyList<Order> orders)
    {
        var result = new List<OrderDetailDto>(orders.Count);
        for (var i = 0; i < orders.Count; i++)
        {
            result.Add(_mapper.Map<Order, OrderDetailDto>(orders[i])!);
        }//for

        return result;
    }//MapOrderDetails

    public RuntimeMappingWorkloadResult MapFullRuntimeWorkload(IReadOnlyList<Customer> customers, IReadOnlyList<Order> orders)
    {
        return new RuntimeMappingWorkloadResult(
            MapCustomers(customers),
            MapOrderSummaries(orders),
            MapOrderDetails(orders));
    }//MapFullRuntimeWorkload

    public Task<List<CustomerFlatDto>> ProjectCustomersAsync(IQueryable<Customer> query, int take, CancellationToken cancellationToken = default)
    {
        return query
            .AsNoTracking()
            .OrderBy(x => x.Id)
            .Take(take)
            .ProjectAs<CustomerFlatDto>(_configuration)
            .ToListAsync(cancellationToken);
    }//ProjectCustomersAsync

    public Task<List<OrderSummaryDto>> ProjectOrderSummariesAsync(IQueryable<Order> query, int take, CancellationToken cancellationToken = default)
    {
        return query
            .AsNoTracking()
            .OrderBy(x => x.Id)
            .Take(take)
            .ProjectAs<OrderSummaryDto>(_configuration)
            .ToListAsync(cancellationToken);
    }//ProjectOrderSummariesAsync

    public Task<List<OrderDetailDto>> ProjectOrderDetailsAsync(IQueryable<Order> query, int take, CancellationToken cancellationToken = default)
    {
        return query
            .AsNoTracking()
            .OrderBy(x => x.Id)
            .Take(take)
            .ProjectAs<OrderDetailDto>(_configuration)
            .ToListAsync(cancellationToken);
    }//ProjectOrderDetailsAsync
}
