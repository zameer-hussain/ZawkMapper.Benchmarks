<p align="center">
  <img src="assets/zawktech-logo.png" alt="ZawkTech Logo" width="420" />
</p>

<p align="center">
  <img src="assets/zawkmapper-logo.png" alt="ZawkMapper Logo" width="520" />
</p>

# ZawkMapper Benchmark Comparison

**ZawkMapper** is a free .NET object mapper and EF Core projection library by **ZawkTech**. This benchmark compares runtime DTO mapping and SQL-friendly `IQueryable` projection across Manual Mapping, AutoMapper, and ZawkMapper `0.6.1-rc.1`.

This repository is meant to keep the comparison open. Developers can run the same benchmark, inspect the scenarios, and share feedback for future performance improvements.

## Package and links

| Resource | Link |
|---|---|
| NuGet package | https://www.nuget.org/packages/ZawkMapper/0.6.1-rc.1 |
| NuGet package page | https://www.nuget.org/packages/ZawkMapper |
| ZawkTech website | https://www.zawktech.com |
| ZawkMapper product page | https://www.zawktech.com/zawkmapper |
| Founder LinkedIn | https://www.linkedin.com/in/zameer-vighio/ |
| ZawkTech LinkedIn | https://www.linkedin.com/company/zawktech |
| GitHub | https://github.com/zameer-hussain/ZawkMapper |

> If the ZawkTech LinkedIn page slug changes, update the URL before publishing.

## Why these results should include laptop specs

Benchmark numbers depend on CPU, RAM, disk, operating system, .NET SDK, background processes, and database state. For public comparison, machine details should be shown with the result.

Fill this section before publishing the benchmark publicly:

| Item | Value |
|---|---|
| Machine | Local Windows laptop |
| CPU | `add CPU model here` |
| RAM | `add RAM here` |
| Storage | `add SSD/NVMe/HDD here` |
| OS | `add Windows version here` |
| .NET SDK | `dotnet --info` |
| Build mode | Release |
| Package tested | ZawkMapper `0.6.1-rc.1` from NuGet |

## Benchmark command

```cmd
for /d /r %i in (bin,obj) do @if exist "%i" rmdir /s /q "%i"
dotnet restore
dotnet build -c Release
dotnet run --project .\src\ZawkMapper.Benchmarks\ZawkMapper.Benchmarks.csproj -- compare --take 100000
```

## Scenario setup

The benchmark separates runtime object mapping from EF Core projection.

| Scenario | Records | What it checks |
|---|---:|---|
| Runtime flat customer DTO | 100,000 customers | simple DTO mapping for flat fields |
| Runtime order summary DTO | 50,000 orders | summary DTO with computed fields |
| Runtime nested order detail DTO | 50,000 orders | nested object and collection mapping |
| Runtime cumulative | mixed | combined runtime workload |
| EF projection flat customer DTO | 100,000 customers | `IQueryable` projection with flat fields |
| EF projection order summary DTO | 50,000 orders | SQL-friendly projection for summary data |
| EF projection nested order detail DTO | 10,000 projected rows | nested projection shape |

The benchmark writes checksum values for each mapper so the result is not only fast, but also mapped correctly.

## Latest published-package result

These numbers were measured using the published NuGet package `ZawkMapper 0.6.1-rc.1`.

### Runtime mapping, elapsed time

Lower is better.

| Scenario | Manual Mapping | AutoMapper | ZawkMapper Mixed | ZawkMapper best variant in this run |
|---|---:|---:|---:|---:|
| Flat customer DTO | 20.320 ms | 37.250 ms | 46.576 ms | 40.501 ms (`MapFieldStrict`) |
| Order summary DTO | 26.822 ms | 61.011 ms | 51.815 ms | 48.853 ms (`MapFieldStrict`) |
| Nested order detail DTO | 39.419 ms | 66.760 ms | 109.328 ms | 109.328 ms (`Mixed`) |
| Cumulative runtime | 91.171 ms | 168.771 ms | 203.371 ms | 197.542 ms (`MapField`) |

### Runtime mapping, allocated memory

Lower is better.

| Scenario | Manual Mapping | AutoMapper | ZawkMapper Mixed |
|---|---:|---:|---:|
| Flat customer DTO | 11.37 MB | 16.19 MB | 14.91 MB |
| Order summary DTO | 7.59 MB | 13.10 MB | 12.43 MB |
| Nested order detail DTO | 18.27 MB | 24.60 MB | 40.31 MB |
| Cumulative runtime | 37.23 MB | 53.59 MB | 67.53 MB |

### EF Core projection, elapsed time

Lower is better.

| Scenario | Manual Select | AutoMapper ProjectTo | ZawkMapper ProjectAs |
|---|---:|---:|---:|
| Flat customer DTO | 261.951 ms | 261.637 ms | 251.886 ms |
| Order summary DTO | 255.560 ms | 211.251 ms | 205.980 ms |
| Nested order detail DTO | 383.717 ms | 378.790 ms | 379.275 ms |

## What the result means

ZawkMapper `0.6.1-rc.1` shows strong progress in runtime mapping and remains very competitive in EF Core projection.

- `ProjectAs` is strong for SQL-friendly projection scenarios.
- Runtime summary mapping beats AutoMapper in this benchmark run.
- Runtime flat mapping has lower allocation than AutoMapper, with close timing.
- Runtime nested collection mapping is the next area for focused optimization.

## ZawkMapper runtime variants

| Variant | Best use |
|---|---|
| `MapFieldStrict` | same-type source and destination members where compile-time type safety is desired |
| `MapFieldDirect` | direct assignment style where the developer wants no conversion responsibility |
| `MapField` | conversion, computed fields, nested object mapping, and collection bridges |
| `Mixed` | realistic application style with direct/simple fields plus flexible computed/nested fields |

Example:

```csharp
cfg.MapModel<Customer, CustomerDto>()
   .MapFieldStrict(d => d.Id, s => s.Id)
   .MapFieldStrict(d => d.Name, s => s.Name)
   .MapField(d => d.DisplayName, s => s.FirstName + " " + s.LastName);
```

For nested collection bridges, use `MapField` on the parent and define a child map:

```csharp
cfg.MapModel<Order, OrderDetailDto>()
   .MapField(d => d.Lines, s => s.Items);

cfg.MapModel<OrderItem, OrderLineDto>()
   .MapFieldStrict(d => d.ProductName, s => s.ProductName)
   .MapFieldStrict(d => d.Quantity, s => s.Quantity)
   .MapFieldStrict(d => d.UnitPrice, s => s.UnitPrice);
```

## Professional benchmark note

This benchmark is not a claim that one mapper wins in every application. It is a transparent comparison for these specific DTO and projection scenarios. Real results can change with data shape, hardware, .NET version, database provider, mapping rules, and application architecture.

## ZawkTech commitment

ZawkMapper is a ZawkTech developer tool. The current public package line is planned to remain free for the developer community while performance, documentation, and real-world mapping scenarios continue to improve.

Useful SEO keywords: .NET object mapper, C# object mapper, DTO mapping, AutoMapper alternative, EF Core projection, IQueryable projection, runtime mapping, nested object mapping, collection mapping, MapModel, ProjectModel, ProjectAs, MapFieldStrict, MapFieldDirect, ZawkMapper, ZawkTech.
