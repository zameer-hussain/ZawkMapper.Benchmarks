namespace ZawkMapper.Benchmarks.Dtos;

public sealed class CustomerFlatDto
{
    public int Id { get; set; }
    public string CustomerCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}

public sealed class OrderSummaryDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public int ItemsCount { get; set; }

    // SQLite cannot translate Sum(decimal) in EF Core projection queries.
    // This benchmark DTO intentionally uses double for projection-safe totals.
    public double Total { get; set; }

    public string Status { get; set; } = string.Empty;
    public DateTime OrderDateUtc { get; set; }
}

public sealed class OrderDetailDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;

    // SQLite cannot translate Sum(decimal) in EF Core projection queries.
    // This benchmark DTO intentionally uses double for projection-safe totals.
    public double Total { get; set; }

    public List<OrderLineDto> Lines { get; set; } = new();
}

public sealed class OrderLineDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public double UnitPrice { get; set; }
    public double LineTotal { get; set; }
}
