using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using ZawkMapper.Benchmarks.Dtos;
using ZawkMapper.Benchmarks.Models;

namespace ZawkMapper.Benchmarks.Mapping;

public sealed class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<Customer, CustomerFlatDto>()
            .ForMember(d => d.FullName, opt => opt.MapFrom(s => s.FirstName + " " + s.LastName))
            .ForMember(d => d.City, opt => opt.MapFrom(s => s.Address == null ? string.Empty : s.Address.City))
            .ForMember(d => d.Country, opt => opt.MapFrom(s => s.Address == null ? string.Empty : s.Address.Country));

        CreateMap<Order, OrderSummaryDto>()
            .ForMember(d => d.CustomerName, opt => opt.MapFrom(s => s.Customer.FirstName + " " + s.Customer.LastName))
            .ForMember(d => d.ItemsCount, opt => opt.MapFrom(s => s.Items.Count))
            .ForMember(d => d.Total, opt => opt.MapFrom(s => s.Items.Sum(x => x.Quantity * (double)x.UnitPrice)));

        CreateMap<OrderItem, OrderLineDto>()
            .ForMember(d => d.ProductName, opt => opt.MapFrom(s => s.Product.Name))
            .ForMember(d => d.Category, opt => opt.MapFrom(s => s.Product.Category))
            .ForMember(d => d.UnitPrice, opt => opt.MapFrom(s => (double)s.UnitPrice))
            .ForMember(d => d.LineTotal, opt => opt.MapFrom(s => s.Quantity * (double)s.UnitPrice));

        CreateMap<Order, OrderDetailDto>()
            .ForMember(d => d.CustomerName, opt => opt.MapFrom(s => s.Customer.FirstName + " " + s.Customer.LastName))
            .ForMember(d => d.City, opt => opt.MapFrom(s => s.Customer.Address == null ? string.Empty : s.Customer.Address.City))
            .ForMember(d => d.Country, opt => opt.MapFrom(s => s.Customer.Address == null ? string.Empty : s.Customer.Address.Country))
            .ForMember(d => d.Total, opt => opt.MapFrom(s => s.Items.Sum(x => x.Quantity * (double)x.UnitPrice)))
            .ForMember(d => d.Lines, opt => opt.MapFrom(s => s.Items));
    }
}

public static class AutoMapperFactory
{
    public static IMapper Create()
    {
        var configuration = new MapperConfiguration(cfg => cfg.AddProfile<AutoMapperProfile>(), NullLoggerFactory.Instance);
        configuration.AssertConfigurationIsValid();
        return configuration.CreateMapper();
    }
}
