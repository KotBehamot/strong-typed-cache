using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using Cache;

namespace StrongTypedCache.Benchmarks;

/// <summary>
/// Benchmarks for GetAllValues operation testing cache enumeration performance.
/// Tests scenarios with varying cache fill rates and expiration conditions.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(RunStrategy.Throughput, warmupCount: 3, iterationCount: 10)]
[MinColumn, MaxColumn, MeanColumn, MedianColumn]
[RankColumn]
public class GetAllValuesBenchmarks
{
    private InMemoryCache<int, BenchmarkValue>? _cache;
    private InMemoryCache<int, BenchmarkValue>? _cacheWithExpired;
    private InMemoryCache<int, LargeValue>? _cacheWithLargeValues;

    [Params(10, 100, 1_000, 10_000)]
    public int DataSize { get; set; }

    [GlobalSetup]
    public void Setup()
    {
    // Cache with all valid entries
    _cache = new InMemoryCache<int, BenchmarkValue>(3600);
        for (int i = 0; i < DataSize; i++)
        {
    _cache.CreateEntry(i, new BenchmarkValue 
            { 
    Data = $"Value_{i}", 
   Timestamp = DateTime.UtcNow, 
     Counter = i 
          });
        }

        // Cache with mix of valid and expired entries
        _cacheWithExpired = new InMemoryCache<int, BenchmarkValue>(1); // 1 second expiration
  for (int i = 0; i < DataSize; i++)
        {
   _cacheWithExpired.CreateEntry(i, new BenchmarkValue 
{ 
        Data = $"Value_{i}", 
        Timestamp = DateTime.UtcNow, 
     Counter = i 
       });
            
         // Let half of them expire
      if (i == DataSize / 2)
            {
  Thread.Sleep(1100);
            }
        }

        // Cache with larger value objects to test memory pressure
        _cacheWithLargeValues = new InMemoryCache<int, LargeValue>(3600);
        for (int i = 0; i < DataSize; i++)
        {
  _cacheWithLargeValues.CreateEntry(i, new LargeValue 
        { 
    Data = new string('X', 1024), // 1KB string
    Buffer = new byte[1024], // 1KB buffer
    Metadata = Enumerable.Range(0, 100).ToDictionary(x => $"Key_{x}", x => $"Value_{x}")
     });
      }
    }

    [Benchmark(Baseline = true)]
    public List<BenchmarkValue?> GetAllValues_AllValid()
    {
        return _cache!.GetAllValues();
    }

    [Benchmark]
    public List<BenchmarkValue?> GetAllValues_WithExpired()
    {
        // This tests cleanup performance when expired entries exist
      return _cacheWithExpired!.GetAllValues();
    }

  [Benchmark]
    public List<LargeValue?> GetAllValues_LargeValues()
    {
   // Tests memory allocation and GC pressure
    return _cacheWithLargeValues!.GetAllValues();
    }

    [Benchmark]
    public int GetAllValues_CountOnly()
    {
        // Scenario where only count is needed
        return _cache!.GetAllValues().Count;
  }

    [Benchmark]
    public List<BenchmarkValue?> GetAllValues_Repeated()
    {
     // Test repeated enumeration (realistic for monitoring scenarios)
        List<BenchmarkValue?> result = null!;
      for (int i = 0; i < 10; i++)
        {
        result = _cache!.GetAllValues();
        }
        return result;
    }

    [Benchmark]
    public BenchmarkValue? GetAllValues_FirstValue()
    {
        // Test when only first value is needed
        var all = _cache!.GetAllValues();
      return all.Count > 0 ? all[0] : null;
    }

    [Benchmark]
    public List<BenchmarkValue?> GetAllValues_FilteredByCondition()
    {
        // Test enumeration with filtering
     var all = _cache!.GetAllValues();
        return all.Where(v => v != null && v.Counter % 2 == 0).ToList();
    }

    [GlobalCleanup]
 public void Cleanup()
    {
        _cache = null;
        _cacheWithExpired = null;
      _cacheWithLargeValues = null;
    }
}

public class LargeValue
{
    public string Data { get; set; } = string.Empty;
    public byte[] Buffer { get; set; } = Array.Empty<byte>();
    public Dictionary<string, string> Metadata { get; set; } = new();
}
