namespace ZawkMapper.Benchmarks.Configuration;

public sealed record BenchmarkRunOptions(
    string Command,
    int Customers,
    int Products,
    int Orders,
    int OrderItems,
    int Take,
    string DatabasePath,
    string ResultsDirectory)
{
    public static BenchmarkRunOptions Parse(string[] args)
    {
        var command = args.FirstOrDefault(a => !a.StartsWith("--", StringComparison.Ordinal)) ?? "compare";

        int GetInt(string name, int fallback)
        {
            var index = Array.IndexOf(args, name);
            if (index >= 0 && index + 1 < args.Length && int.TryParse(args[index + 1], out var value))
            {
                return value;
            }
            return fallback;
        }

        string GetString(string name, string fallback)
        {
            var index = Array.IndexOf(args, name);
            return index >= 0 && index + 1 < args.Length ? args[index + 1] : fallback;
        }

        var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
        var databasePath = GetString("--db", Path.Combine(root, "results", "zawkmapper-benchmark.db"));
        var resultsDirectory = GetString("--results", Path.Combine(root, "results"));

        return new BenchmarkRunOptions(
            Command: command.ToLowerInvariant(),
            Customers: GetInt("--customers", 100_000),
            Products: GetInt("--products", 500),
            Orders: GetInt("--orders", 100_000),
            OrderItems: GetInt("--items", 200_000),
            Take: GetInt("--take", 100_000),
            DatabasePath: databasePath,
            ResultsDirectory: resultsDirectory);
    }

    public static string HelpText => """
    ZawkMapper Benchmarks

    Commands:
      seed        Create SQLite database and seed data.
      compare     Run stopwatch-based comparison and export Excel/CSV, including separate ZawkMapper MapField/Direct/Strict/Mixed runtime variants.
      benchmark   Run BenchmarkDotNet benchmarks.

    Examples:
      dotnet run -- seed --customers 100000
      dotnet run -- compare --take 100000
      dotnet run -- compare --take 25000 --results ./results-before-061
      dotnet run -c Release -- benchmark

    Options:
      --customers <number>   Default: 100000
      --products <number>    Default: 500
      --orders <number>      Default: 100000
      --items <number>       Default: 200000
      --take <number>        Default: 100000
      --db <path>            SQLite database path
      --results <path>       Results output folder
    """;
}
