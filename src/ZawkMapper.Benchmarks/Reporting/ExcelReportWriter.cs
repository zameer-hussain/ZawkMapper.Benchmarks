using ClosedXML.Excel;

namespace ZawkMapper.Benchmarks.Reporting;

public static class ExcelReportWriter
{
    public static void Write(IReadOnlyList<ComparisonMetric> metrics, string resultsDirectory)
    {
        Directory.CreateDirectory(resultsDirectory);
        WriteCsv(metrics, Path.Combine(resultsDirectory, "MapperComparison.csv"));
        WriteWorkbook(metrics, Path.Combine(resultsDirectory, "MapperComparison.xlsx"));
    }//Write

    private static void WriteCsv(IReadOnlyList<ComparisonMetric> metrics, string path)
    {
        using var writer = new StreamWriter(path);
        writer.WriteLine("Scenario,Mapper,Records,ElapsedMilliseconds,AllocatedBytes,AllocatedMB,MicrosecondsPerRecord,BytesPerRecord,Checksum,MeasuredUtc");
        foreach (var metric in metrics)
        {
            writer.WriteLine($"{Escape(metric.Scenario)},{Escape(metric.Mapper)},{metric.Records},{metric.ElapsedMilliseconds:0.000},{metric.AllocatedBytes},{metric.AllocatedMegabytes:0.000},{metric.MicrosecondsPerRecord:0.000},{metric.BytesPerRecord:0.00},{Escape(metric.Checksum)},{metric.MeasuredUtc:O}");
        }//foreach
    }//WriteCsv

    private static void WriteWorkbook(IReadOnlyList<ComparisonMetric> metrics, string path)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Results");
        sheet.Cell(1, 1).Value = "Scenario";
        sheet.Cell(1, 2).Value = "Mapper";
        sheet.Cell(1, 3).Value = "Records";
        sheet.Cell(1, 4).Value = "Elapsed ms";
        sheet.Cell(1, 5).Value = "Allocated bytes";
        sheet.Cell(1, 6).Value = "Allocated MB";
        sheet.Cell(1, 7).Value = "Microseconds/record";
        sheet.Cell(1, 8).Value = "Bytes/record";
        sheet.Cell(1, 9).Value = "Checksum";
        sheet.Cell(1, 10).Value = "Measured UTC";

        for (var i = 0; i < metrics.Count; i++)
        {
            var row = i + 2;
            var metric = metrics[i];
            sheet.Cell(row, 1).Value = metric.Scenario;
            sheet.Cell(row, 2).Value = metric.Mapper;
            sheet.Cell(row, 3).Value = metric.Records;
            sheet.Cell(row, 4).Value = metric.ElapsedMilliseconds;
            sheet.Cell(row, 5).Value = metric.AllocatedBytes;
            sheet.Cell(row, 6).Value = metric.AllocatedMegabytes;
            sheet.Cell(row, 7).Value = metric.MicrosecondsPerRecord;
            sheet.Cell(row, 8).Value = metric.BytesPerRecord;
            sheet.Cell(row, 9).Value = metric.Checksum;
            sheet.Cell(row, 10).Value = metric.MeasuredUtc;
        }//for

        var range = sheet.RangeUsed();
        if (range is not null)
        {
            range.CreateTable();
            sheet.Columns().AdjustToContents();
            sheet.Row(1).Style.Font.Bold = true;
        }//if range

        var summary = workbook.Worksheets.Add("Summary");
        summary.Cell(1, 1).Value = "How to read this file";
        summary.Cell(2, 1).Value = "Lower elapsed milliseconds is better. Lower allocated bytes, allocated MB, microseconds/record, and bytes/record are better.";
        summary.Cell(4, 1).Value = "Important";
        summary.Cell(5, 1).Value = "Benchmark results depend on machine, .NET version, package versions, database provider, and data shape. Re-run locally before making final claims.";
        summary.Cell(7, 1).Value = "ZawkMapper runtime variants";
        summary.Cell(8, 1).Value = "MapField uses flexible mapping and conversion checks. MapFieldDirect uses direct assignment where valid. MapFieldStrict uses same-type compile-time member mapping where valid. Mixed is the current realistic style.";
        summary.Cell(10, 1).Value = "Nested collection note";
        summary.Cell(11, 1).Value = "Order.Items -> OrderDetailDto.Lines must use the flexible collection bridge because List<OrderItem> is not directly assignable to List<OrderLineDto>. Child item mapping still changes per ZawkMapper variant.";
        summary.Columns().AdjustToContents();

        workbook.SaveAs(path);
    }//WriteWorkbook

    private static string Escape(string value)
    {
        if (!value.Contains(',') && !value.Contains('"') && !value.Contains('\n'))
        {
            return value;
        }//if no escape needed

        return $"\"{value.Replace("\"", "\"\"")}\"";
    }//Escape
}
