using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Csv;

namespace StrongTypedCache.Benchmarks;

/// <summary>
/// Global benchmark configuration for consistent reporting across all benchmark suites.
/// </summary>
public class BenchmarkConfig : ManualConfig
{
    public BenchmarkConfig()
    {
        // Add multiple exporters for different consumption scenarios
        AddExporter(HtmlExporter.Default); // Human-readable HTML reports
        AddExporter(MarkdownExporter.GitHub); // GitHub-friendly markdown
   AddExporter(CsvExporter.Default); // Data analysis in Excel/Python
      AddExporter(RPlotExporter.Default); // Plots for visualization

        // Configure summary style
      WithSummaryStyle(BenchmarkDotNet.Reports.SummaryStyle.Default
            .WithRatioStyle(BenchmarkDotNet.Columns.RatioStyle.Trend)
    .WithMaxParameterColumnWidth(50));
    }
}
