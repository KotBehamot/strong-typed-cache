# StrongTypedCache Benchmarks

This project contains comprehensive performance benchmarks for the StrongTypedCache library using BenchmarkDotNet.

## Overview

The benchmark suite tests cache performance across multiple dimensions:
- **Key Types**: int, string, Guid, complex objects
- **Data Scales**: 10, 100, 1K, 10K, 100K entries
- **Operations**: CreateEntry, TryGetValue, GetAllValues, Remove, Mixed
- **Scenarios**: Real-world patterns (session cache, monitoring, LRU, etc.)
- **Memory**: Allocation patterns, GC pressure, object sizes

## Running Benchmarks

### Run All Benchmarks
```bash
cd StrongTypedCache.Benchmarks
dotnet run -c Release
```

### Run Specific Benchmark Class
```bash
# Use namespace.ClassName syntax (NOT wildcards!)
dotnet run -c Release --filter StrongTypedCache.Benchmarks.CreateEntryBenchmarks
dotnet run -c Release --filter StrongTypedCache.Benchmarks.TryGetValueBenchmarks
```

### Run Specific Benchmark Method
```bash
# Full path: namespace.ClassName.MethodName
dotnet run -c Release --filter StrongTypedCache.Benchmarks.CreateEntryBenchmarks.CreateEntry_IntKey
```

### Run Multiple Specific Benchmarks
```bash
# Use wildcards ONLY in method names (not with --filter)
dotnet run -c Release --filter *CreateEntry*  # This often FAILS
# Instead, run the whole class or use --anyCategories/--allCategories
```

### Quick Dry Run (Fast Validation)
```bash
cd StrongTypedCache.Benchmarks
dotnet run -c Release -- --job dry
```

### Run with Specific Configuration
```bash
dotnet run -c Release -- --job short --memory
```

### List All Available Benchmarks
```bash
# Tree view
dotnet run -c Release -- --list tree

# Flat list (useful for scripting)
dotnet run -c Release -- --list flat
```

### ?? Important: BenchmarkDotNet Filter Syntax

BenchmarkDotNet filters work on **FULL QUALIFIED NAMES**, not wildcards:

```bash
# ? CORRECT - Full namespace
dotnet run -c Release --filter StrongTypedCache.Benchmarks.CreateEntryBenchmarks

# ? CORRECT - Full path with method
dotnet run -c Release --filter StrongTypedCache.Benchmarks.CreateEntryBenchmarks.CreateEntry_IntKey

# ? WRONG - Wildcard with --filter
dotnet run -c Release -- --filter *CreateEntry*  # Returns 0 benchmarks!

# ? ALTERNATIVE - No filter runs ALL
dotnet run -c Release

# ? ALTERNATIVE - Use job for quick tests
dotnet run -c Release -- --job dry
```

## Benchmark Suites

### 1. CreateEntryBenchmarks
Tests cache write performance with:
- Multiple key types (int, string, Guid, complex)
- Various data sizes (10 to 10K entries)
- Overwrite scenarios

**Key Metrics**: Throughput (ops/sec), Memory allocations

### 2. TryGetValueBenchmarks
Tests cache read performance with:
- 100% hits (all keys exist)
- 0% hits (all keys missing)
- 50% mixed hit/miss ratio
- Sequential vs random access patterns
- Cache locality (repeated same key access)

**Key Metrics**: Latency (ns/op), Hit rate impact

### 3. GetAllValuesBenchmarks
Tests cache enumeration with:
- All valid entries
- Mix of valid and expired entries
- Large value objects (memory pressure)
- Filtered results scenarios

**Key Metrics**: Time to enumerate, Memory allocations, GC collections

### 4. RemoveBenchmarks
Tests cache deletion with:
- Sequential vs random removal
- Partial removal patterns
- Non-existent key removal
- Remove-and-reinsert patterns

**Key Metrics**: Throughput, Memory cleanup efficiency

### 5. MixedOperationBenchmarks
Realistic workload patterns:
- **Read-Heavy** (90% read, 10% write) - typical web apps
- **Write-Heavy** (70% write, 30% read) - bulk updates
- **Balanced CRUD** (25% each operation)
- **Session Cache** - create, multiple reads, remove
- **High Churn** - rapid create/remove cycles
- **Monitoring** - periodic full enumeration + updates
- **LRU Simulation** - hot/cold data access patterns

**Key Metrics**: Mixed operation throughput, Real-world performance

### 6. MemoryAllocationBenchmarks
Memory behavior analysis:
- Small (< 100B), Medium (~1KB), Large (~10KB) values
- Short vs long expiration impact
- Repeated overwrite memory patterns
- GetAllValues allocation overhead
- ConcurrentDictionary growth patterns

**Key Metrics**: Allocated bytes, Gen0/Gen1/Gen2 collections, Working set

## Interpreting Results

### Output Files
After running, results are available in `BenchmarkDotNet.Artifacts/results/`:
- `*-report.html` - Interactive HTML report with charts
- `*-report-github.md` - Markdown summary for GitHub
- `*.csv` - Raw data for custom analysis
- `*-measurements.csv` - Detailed measurements per iteration

### Key Metrics to Watch

1. **Mean Time**: Average operation duration
2. **Allocated**: Memory allocated per operation
3. **Gen0/Gen1/Gen2**: Garbage collection frequency
4. **Rank**: Relative performance (1 = fastest)
5. **Ratio**: Performance relative to baseline

### Performance Targets

Reasonable targets for a well-performing cache:
- **CreateEntry**: < 500 ns/op for int keys, < 1 ?s for string keys
- **TryGetValue (hit)**: < 100 ns/op
- **TryGetValue (miss)**: < 50 ns/op
- **Remove**: < 200 ns/op
- **GetAllValues**: < 50 ?s for 1K entries

## CI Integration

Benchmarks can be integrated into CI/CD:

```yaml
- name: Run Benchmarks
  run: |
    cd StrongTypedCache.Benchmarks
    dotnet run -c Release -- --filter *CreateEntry* --exporters json
```

## Customization

### Add Custom Scenarios
Create new benchmark class:
```csharp
[MemoryDiagnoser]
[SimpleJob(RunStrategy.Throughput)]
public class MyCustomBenchmarks
{
 [Benchmark]
    public void MyScenario() { /* ... */ }
}
```

### Adjust Data Sizes
Modify `[Params]` attribute:
```csharp
[Params(50, 500, 5_000)] // Custom sizes
public int DataSize { get; set; }
```

### Compare Implementations
Add baseline for comparison:
```csharp
[Benchmark(Baseline = true)]
public void CurrentImplementation() { }

[Benchmark]
public void NewImplementation() { }
```

## Troubleshooting

### Benchmarks Take Too Long
Use shorter job:
```bash
dotnet run -c Release -- --job dry
```

### Memory Issues
Reduce data sizes or run subsets:
```bash
dotnet run -c Release -- --filter *Benchmarks.Memory_SmallValues*
```

### Inconsistent Results
Ensure:
- Running in Release mode
- No other intensive processes running
- Sufficient warmup iterations
- Stable system state (no thermal throttling)

## Best Practices

1. **Always run in Release mode** - Debug builds have significant overhead
2. **Close other applications** - Reduce system noise
3. **Run multiple times** - Verify consistency
4. **Compare baseline** - Track performance over time
5. **Monitor memory** - Not just speed, but efficiency

## Contributing

When adding new benchmarks:
1. Use `[MemoryDiagnoser]` attribute
2. Include baseline comparison
3. Test multiple data scales with `[Params]`
4. Document the scenario being tested
5. Add to this README

## References

- [BenchmarkDotNet Documentation](https://benchmarkdotnet.org/)
- [Performance Best Practices](https://docs.microsoft.com/en-us/dotnet/standard/performance/)
