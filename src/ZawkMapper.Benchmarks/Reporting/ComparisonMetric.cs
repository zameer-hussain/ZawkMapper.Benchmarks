namespace ZawkMapper.Benchmarks.Reporting;

public sealed record ComparisonMetric(
    string Scenario,
    string Mapper,
    int Records,
    double ElapsedMilliseconds,
    long AllocatedBytes,
    string Checksum,
    DateTime MeasuredUtc)
{
    public double AllocatedMegabytes => AllocatedBytes / 1024d / 1024d;

    public double MicrosecondsPerRecord => Records <= 0 ? 0 : ElapsedMilliseconds * 1000d / Records;

    public double BytesPerRecord => Records <= 0 ? 0 : AllocatedBytes / (double)Records;
}
