namespace ZawkMapper.Benchmarks.Models;

public sealed class Customer
{
    public int Id { get; set; }
    public string CustomerCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime CreatedUtc { get; set; }
    public Address? Address { get; set; }
    public List<Order> Orders { get; set; } = new();
}

public sealed class Address
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
}

public sealed class Product
{
    public int Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public List<OrderItem> Items { get; set; } = new();
}

public sealed class Order
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public DateTime OrderDateUtc { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<OrderItem> Items { get; set; } = new();
    public Payment? Payment { get; set; }
}

public sealed class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public sealed class Payment
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public string Method { get; set; } = string.Empty;
    public string ProviderReference { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime PaidUtc { get; set; }
}
