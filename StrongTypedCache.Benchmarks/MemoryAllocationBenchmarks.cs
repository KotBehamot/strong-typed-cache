using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using Cache;

namespace StrongTypedCache.Benchmarks;

/// <summary>
/// Benchmarks focused on memory allocation patterns and GC pressure.
/// Tests various expiration scenarios and their memory impact.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(RunStrategy.Throughput, warmupCount: 3, iterationCount: 10)]
[MinColumn, MaxColumn, MeanColumn, MedianColumn]
[RankColumn]
public class MemoryAllocationBenchmarks
{
    private InMemoryCache<int, SmallValue>? _cacheSmallValues;
    private InMemoryCache<int, MediumValue>? _cacheMediumValues;
    private InMemoryCache<int, LargeValue>? _cacheLargeValues;
    private InMemoryCache<int, BenchmarkValue>? _cacheShortExpiration;
    private InMemoryCache<int, BenchmarkValue>? _cacheLongExpiration;

    [Params(100, 1_000, 10_000)]
    public int DataSize { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _cacheSmallValues = new InMemoryCache<int, SmallValue>(3600);
        _cacheMediumValues = new InMemoryCache<int, MediumValue>(3600);
        _cacheLargeValues = new InMemoryCache<int, LargeValue>(3600);
        _cacheShortExpiration = new InMemoryCache<int, BenchmarkValue>(1); // 1 second
        _cacheLongExpiration = new InMemoryCache<int, BenchmarkValue>(3600); // 1 hour
    }

    [Benchmark(Baseline = true)]
    public void Memory_SmallValues_Create()
    {
        // Test memory allocation for small objects (< 100 bytes)
        for (int i = 0; i < DataSize; i++)
        {
    _cacheSmallValues!.CreateEntry(i, new SmallValue { Id = i, Flag = true });
    }
    }

    [Benchmark]
    public void Memory_MediumValues_Create()
    {
        // Test memory allocation for medium objects (~ 1KB)
        for (int i = 0; i < DataSize; i++)
        {
  _cacheMediumValues!.CreateEntry(i, new MediumValue 
            { 
 Id = i, 
       Data = new string('X', 256),
    Metadata = Enumerable.Range(0, 10).ToDictionary(x => $"K{x}", x => $"V{x}")
      });
    }
 }

    [Benchmark]
    public void Memory_LargeValues_Create()
    {
 // Test memory allocation for large objects (~ 10KB)
      for (int i = 0; i < DataSize; i++)
        {
     _cacheLargeValues!.CreateEntry(i, new LargeValue 
 { 
            Data = new string('X', 1024),
     Buffer = new byte[8192],
      Metadata = Enumerable.Range(0, 100).ToDictionary(x => $"Key_{x}", x => $"Value_{x}")
            });
        }
    }

    [Benchmark]
    public void Memory_ShortExpiration_ChurnRate()
    {
        // Test memory pressure with frequent expirations
        for (int cycle = 0; cycle < 5; cycle++)
        {
       for (int i = 0; i < DataSize / 5; i++)
    {
   _cacheShortExpiration!.CreateEntry(i, new BenchmarkValue 
          { 
        Data = $"Cycle_{cycle}_Value_{i}", 
Timestamp = DateTime.UtcNow, 
     Counter = i 
    });
      }
        Thread.Sleep(300); // Let some entries expire
     var allValues = _cacheShortExpiration.GetAllValues(); // Trigger cleanup
        }
    }

    [Benchmark]
    public void Memory_LongExpiration_Accumulation()
    {
  // Test memory accumulation with long-lived entries
        for (int i = 0; i < DataSize; i++)
   {
            _cacheLongExpiration!.CreateEntry(i, new BenchmarkValue 
   { 
        Data = $"LongLived_{i}", 
     Timestamp = DateTime.UtcNow, 
    Counter = i 
});
   }
        
        // Keep accessing to ensure no GC
   for (int i = 0; i < DataSize; i++)
    {
          var found = _cacheLongExpiration.TryGetValue(i, out var _);
  }
    }

    [Benchmark]
    public void Memory_Overwrite_SameKeys()
    {
        // Test memory behavior when repeatedly overwriting same keys
for (int cycle = 0; cycle < 10; cycle++)
{
            for (int i = 0; i < DataSize / 10; i++)
 {
    _cacheSmallValues!.CreateEntry(i, new SmallValue 
    { 
  Id = i * cycle, 
Flag = cycle % 2 == 0 
        });
          }
      }
    }

    [Benchmark]
    public void Memory_CreateReadRemove_Cycle()
    {
        // Test full lifecycle memory impact
 for (int i = 0; i < DataSize; i++)
        {
    var key = i;
   
          // Create
        _cacheSmallValues!.CreateEntry(key, new SmallValue { Id = key, Flag = true });
        
  // Read
      _cacheSmallValues.TryGetValue(key, out var _);
            
 // Remove
            _cacheSmallValues.Remove(key);
        }
 }

    [Benchmark]
    public void Memory_GetAllValues_Allocation()
    {
        // Pre-populate cache
        for (int i = 0; i < DataSize; i++)
        {
    _cacheSmallValues!.CreateEntry(i, new SmallValue { Id = i, Flag = true });
        }

   // Test allocation impact of GetAllValues
        for (int i = 0; i < 10; i++)
        {
  var values = _cacheSmallValues.GetAllValues();
 }
    }

    [Benchmark]
    public void Memory_ConcurrentDictionary_GrowthPattern()
    {
        // Test memory behavior as cache grows
        var cache = new InMemoryCache<int, SmallValue>(3600);
        
        // Grow in batches to simulate real-world patterns
        for (int batch = 0; batch < 10; batch++)
 {
         for (int i = 0; i < DataSize / 10; i++)
   {
    cache.CreateEntry(batch * (DataSize / 10) + i, new SmallValue 
              { 
  Id = i, 
        Flag = batch % 2 == 0 
            });
      }
        }
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _cacheSmallValues = null;
   _cacheMediumValues = null;
        _cacheLargeValues = null;
        _cacheShortExpiration = null;
        _cacheLongExpiration = null;
  }
}

public class SmallValue
{
    public int Id { get; set; }
    public bool Flag { get; set; }
}

public class MediumValue
{
    public int Id { get; set; }
    public string Data { get; set; } = string.Empty;
    public Dictionary<string, string> Metadata { get; set; } = new();
}
