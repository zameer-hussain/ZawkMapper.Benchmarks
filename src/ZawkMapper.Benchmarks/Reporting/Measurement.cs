using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using ZawkMapper.Benchmarks.Dtos;

namespace ZawkMapper.Benchmarks.Reporting;

public static class Measurement
{
    public static ComparisonMetric Run<T>(string scenario, string mapperName, int records, Func<IReadOnlyList<T>> operation)
    {
        return RunCore(scenario, mapperName, records, operation);
    }//Run

    public static ComparisonMetric RunValue<T>(string scenario, string mapperName, int records, Func<T> operation)
    {
        return RunCore(scenario, mapperName, records, operation);
    }//RunValue

    private static ComparisonMetric RunCore<T>(string scenario, string mapperName, int records, Func<T> operation)
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var before = GC.GetTotalAllocatedBytes(precise: true);
        var sw = Stopwatch.StartNew();
        var result = operation();
        sw.Stop();
        var after = GC.GetTotalAllocatedBytes(precise: true);

        return new ComparisonMetric(
            Scenario: scenario,
            Mapper: mapperName,
            Records: records,
            ElapsedMilliseconds: sw.Elapsed.TotalMilliseconds,
            AllocatedBytes: after - before,
            Checksum: ComputeChecksum(result),
            MeasuredUtc: DateTime.UtcNow);
    }//RunCore

    private static string ComputeChecksum<T>(T result)
    {
        var builder = new StringBuilder(1024);
        AppendChecksum(builder, result);
        var bytes = Encoding.UTF8.GetBytes(builder.ToString());
        return Convert.ToHexString(SHA256.HashData(bytes))[..16];
    }//ComputeChecksum

    private static void AppendChecksum(StringBuilder builder, object? value)
    {
        switch (value)
        {
            case RuntimeMappingWorkloadResult workload:
                builder.Append("workload|");
                AppendCustomerChecksum(builder, workload.Customers);
                AppendOrderSummaryChecksum(builder, workload.OrderSummaries);
                AppendOrderDetailChecksum(builder, workload.OrderDetails);
                break;

            case IReadOnlyList<CustomerFlatDto> customers:
                AppendCustomerChecksum(builder, customers);
                break;

            case IReadOnlyList<OrderSummaryDto> orderSummaries:
                AppendOrderSummaryChecksum(builder, orderSummaries);
                break;

            case IReadOnlyList<OrderDetailDto> orderDetails:
                AppendOrderDetailChecksum(builder, orderDetails);
                break;

            case IReadOnlyList<OrderLineDto> lines:
                AppendOrderLineChecksum(builder, lines);
                break;

            case null:
                builder.Append("null");
                break;

            default:
                builder.Append(value.GetType().FullName).Append('|').Append(value);
                break;
        }//switch
    }//AppendChecksum

    private static void AppendCustomerChecksum(StringBuilder builder, IReadOnlyList<CustomerFlatDto> customers)
    {
        builder.Append("customers:").Append(customers.Count).Append('|');
        long idSum = 0;
        var textLength = 0;

        for (var i = 0; i < customers.Count; i++)
        {
            var item = customers[i];
            idSum += item.Id;
            textLength += item.CustomerCode.Length + item.FullName.Length + item.Email.Length + item.City.Length + item.Country.Length;
        }//for

        builder.Append(idSum).Append('|').Append(textLength).Append('|');
        AppendFirstLast(builder, customers, x => $"{x.Id}:{x.CustomerCode}:{x.FullName}:{x.City}:{x.Country}");
    }//AppendCustomerChecksum

    private static void AppendOrderSummaryChecksum(StringBuilder builder, IReadOnlyList<OrderSummaryDto> orders)
    {
        builder.Append("summaries:").Append(orders.Count).Append('|');
        long idSum = 0;
        var itemCountSum = 0;
        double totalSum = 0;
        var textLength = 0;

        for (var i = 0; i < orders.Count; i++)
        {
            var item = orders[i];
            idSum += item.Id;
            itemCountSum += item.ItemsCount;
            totalSum += item.Total;
            textLength += item.OrderNumber.Length + item.CustomerName.Length + item.Status.Length;
        }//for

        builder.Append(idSum).Append('|')
            .Append(itemCountSum).Append('|')
            .Append(totalSum.ToString("0.####", CultureInfo.InvariantCulture)).Append('|')
            .Append(textLength).Append('|');
        AppendFirstLast(builder, orders, x => $"{x.Id}:{x.OrderNumber}:{x.CustomerName}:{x.ItemsCount}:{x.Total:0.####}:{x.Status}");
    }//AppendOrderSummaryChecksum

    private static void AppendOrderDetailChecksum(StringBuilder builder, IReadOnlyList<OrderDetailDto> orders)
    {
        builder.Append("details:").Append(orders.Count).Append('|');
        long idSum = 0;
        var lineCountSum = 0;
        double totalSum = 0;
        double lineTotalSum = 0;
        var textLength = 0;

        for (var i = 0; i < orders.Count; i++)
        {
            var item = orders[i];
            idSum += item.Id;
            totalSum += item.Total;
            lineCountSum += item.Lines.Count;
            textLength += item.OrderNumber.Length + item.CustomerName.Length + item.City.Length + item.Country.Length;

            for (var j = 0; j < item.Lines.Count; j++)
            {
                lineTotalSum += item.Lines[j].LineTotal;
                textLength += item.Lines[j].ProductName.Length + item.Lines[j].Category.Length;
            }//for lines
        }//for orders

        builder.Append(idSum).Append('|')
            .Append(lineCountSum).Append('|')
            .Append(totalSum.ToString("0.####", CultureInfo.InvariantCulture)).Append('|')
            .Append(lineTotalSum.ToString("0.####", CultureInfo.InvariantCulture)).Append('|')
            .Append(textLength).Append('|');
        AppendFirstLast(builder, orders, x => $"{x.Id}:{x.OrderNumber}:{x.CustomerName}:{x.Lines.Count}:{x.Total:0.####}");
    }//AppendOrderDetailChecksum

    private static void AppendOrderLineChecksum(StringBuilder builder, IReadOnlyList<OrderLineDto> lines)
    {
        builder.Append("lines:").Append(lines.Count).Append('|');
        long productIdSum = 0;
        var quantitySum = 0;
        double lineTotalSum = 0;
        var textLength = 0;

        for (var i = 0; i < lines.Count; i++)
        {
            var item = lines[i];
            productIdSum += item.ProductId;
            quantitySum += item.Quantity;
            lineTotalSum += item.LineTotal;
            textLength += item.ProductName.Length + item.Category.Length;
        }//for

        builder.Append(productIdSum).Append('|')
            .Append(quantitySum).Append('|')
            .Append(lineTotalSum.ToString("0.####", CultureInfo.InvariantCulture)).Append('|')
            .Append(textLength).Append('|');
    }//AppendOrderLineChecksum

    private static void AppendFirstLast<T>(StringBuilder builder, IReadOnlyList<T> values, Func<T, string> formatter)
    {
        if (values.Count == 0)
        {
            builder.Append("empty|");
            return;
        }//if empty

        builder.Append("first:").Append(formatter(values[0])).Append('|');
        builder.Append("last:").Append(formatter(values[^1])).Append('|');
    }//AppendFirstLast
}
