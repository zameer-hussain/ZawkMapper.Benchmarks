using ZawkMapper.Benchmarks.Dtos;
using ZawkMapper.Benchmarks.Models;

namespace ZawkMapper.Benchmarks.Mapping;

public static class ManualMapper
{
    public static CustomerFlatDto ToCustomerFlatDto(Customer source) => new()
    {
        Id = source.Id,
        CustomerCode = source.CustomerCode,
        FullName = source.FirstName + " " + source.LastName,
        Email = source.Email,
        City = source.Address?.City ?? string.Empty,
        Country = source.Address?.Country ?? string.Empty
    };

    public static OrderSummaryDto ToOrderSummaryDto(Order source) => new()
    {
        Id = source.Id,
        OrderNumber = source.OrderNumber,
        CustomerName = source.Customer.FirstName + " " + source.Customer.LastName,
        ItemsCount = source.Items.Count,
        Total = source.Items.Sum(x => x.Quantity * (double)x.UnitPrice),
        Status = source.Status,
        OrderDateUtc = source.OrderDateUtc
    };

    public static OrderDetailDto ToOrderDetailDto(Order source) => new()
    {
        Id = source.Id,
        OrderNumber = source.OrderNumber,
        CustomerName = source.Customer.FirstName + " " + source.Customer.LastName,
        City = source.Customer.Address?.City ?? string.Empty,
        Country = source.Customer.Address?.Country ?? string.Empty,
        Total = source.Items.Sum(x => x.Quantity * (double)x.UnitPrice),
        Lines = ToOrderLineDtos(source.Items)
    };

    public static List<CustomerFlatDto> ToCustomerFlatDtos(IReadOnlyList<Customer> source)
    {
        var result = new List<CustomerFlatDto>(source.Count);
        for (var i = 0; i < source.Count; i++)
        {
            result.Add(ToCustomerFlatDto(source[i]));
        }//for

        return result;
    }//ToCustomerFlatDtos

    public static List<OrderSummaryDto> ToOrderSummaryDtos(IReadOnlyList<Order> source)
    {
        var result = new List<OrderSummaryDto>(source.Count);
        for (var i = 0; i < source.Count; i++)
        {
            result.Add(ToOrderSummaryDto(source[i]));
        }//for

        return result;
    }//ToOrderSummaryDtos

    public static List<OrderDetailDto> ToOrderDetailDtos(IReadOnlyList<Order> source)
    {
        var result = new List<OrderDetailDto>(source.Count);
        for (var i = 0; i < source.Count; i++)
        {
            result.Add(ToOrderDetailDto(source[i]));
        }//for

        return result;
    }//ToOrderDetailDtos

    public static RuntimeMappingWorkloadResult ToFullRuntimeWorkload(IReadOnlyList<Customer> customers, IReadOnlyList<Order> orders)
    {
        return new RuntimeMappingWorkloadResult(
            ToCustomerFlatDtos(customers),
            ToOrderSummaryDtos(orders),
            ToOrderDetailDtos(orders));
    }//ToFullRuntimeWorkload

    private static List<OrderLineDto> ToOrderLineDtos(IReadOnlyList<OrderItem> source)
    {
        var result = new List<OrderLineDto>(source.Count);
        for (var i = 0; i < source.Count; i++)
        {
            var item = source[i];
            result.Add(new OrderLineDto
            {
                ProductId = item.ProductId,
                ProductName = item.Product.Name,
                Category = item.Product.Category,
                Quantity = item.Quantity,
                UnitPrice = (double)item.UnitPrice,
                LineTotal = item.Quantity * (double)item.UnitPrice
            });
        }//for

        return result;
    }//ToOrderLineDtos
}
