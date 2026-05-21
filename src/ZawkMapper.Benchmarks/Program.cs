using BenchmarkDotNet.Running;
using ZawkMapper.Benchmarks;
using ZawkMapper.Benchmarks.Benchmarks;
using ZawkMapper.Benchmarks.Configuration;
using ZawkMapper.Benchmarks.Data;

Console.OutputEncoding = System.Text.Encoding.UTF8;
var options = BenchmarkRunOptions.Parse(args);

if (args.Contains("--help") || args.Contains("-h"))
{
    Console.WriteLine(BenchmarkRunOptions.HelpText);
    return;
}

try
{
    switch (options.Command)
    {
        case "seed":
            await DatabaseSeeder.EnsureCreatedAndSeededAsync(options);
            break;

        case "benchmark":
            BenchmarkRunner.Run<MapperBenchmarks>();
            break;

        case "compare":
        default:
            await ComparisonJob.RunAsync(options);
            break;
    }
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine(ex);
    Console.ResetColor();
    Environment.ExitCode = 1;
}
