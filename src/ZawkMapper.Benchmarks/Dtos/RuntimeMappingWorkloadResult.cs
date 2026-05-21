namespace ZawkMapper.Benchmarks.Dtos;

/// <summary>
/// Holds a combined runtime workload result so the benchmark can measure flat, summary, and nested mapping together.
/// </summary>
public sealed class RuntimeMappingWorkloadResult
{
    public RuntimeMappingWorkloadResult(
        List<CustomerFlatDto> customers,
        List<OrderSummaryDto> orderSummaries,
        List<OrderDetailDto> orderDetails)
    {
        Customers = customers ?? throw new ArgumentNullException(nameof(customers));
        OrderSummaries = orderSummaries ?? throw new ArgumentNullException(nameof(orderSummaries));
        OrderDetails = orderDetails ?? throw new ArgumentNullException(nameof(orderDetails));
    }//RuntimeMappingWorkloadResult

    public List<CustomerFlatDto> Customers { get; }

    public List<OrderSummaryDto> OrderSummaries { get; }

    public List<OrderDetailDto> OrderDetails { get; }

    public int TotalRecords => Customers.Count + OrderSummaries.Count + OrderDetails.Count;
}
