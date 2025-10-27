using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Exporters.Json;
using BenchmarkDotNet.Jobs;

namespace StrongTypedCache.Benchmarks;

/// <summary>
/// CI-optimized benchmark configuration with shorter iteration counts.
/// Use for automated builds where time is limited.
/// </summary>
public class CIBenchmarkConfig : ManualConfig
{
    public CIBenchmarkConfig()
    {
        // Shorter job for CI - still statistically significant but faster
        AddJob(Job.Default
            .WithStrategy(RunStrategy.Throughput)
            .WithWarmupCount(2)      // Reduced from 3
            .WithIterationCount(5)); // Reduced from 10

        // Add exporters
        AddExporter(HtmlExporter.Default);
        AddExporter(MarkdownExporter.GitHub);
        AddExporter(CsvExporter.Default);
        AddExporter(JsonExporter.BriefCompressed);

        // Configure summary style
        WithSummaryStyle(BenchmarkDotNet.Reports.SummaryStyle.Default
            .WithRatioStyle(BenchmarkDotNet.Columns.RatioStyle.Trend)
            .WithMaxParameterColumnWidth(50));
    }
}
