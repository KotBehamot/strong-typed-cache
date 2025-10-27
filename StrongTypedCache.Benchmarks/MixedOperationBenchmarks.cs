using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using Cache;

namespace StrongTypedCache.Benchmarks;

/// <summary>
/// Benchmarks for realistic mixed-operation scenarios simulating real-world cache usage patterns.
/// Combines Create, Read, Update, Delete operations in various ratios.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(RunStrategy.Throughput, warmupCount: 3, iterationCount: 10)]
[MinColumn, MaxColumn, MeanColumn, MedianColumn]
[RankColumn]
public class MixedOperationBenchmarks
{
    private InMemoryCache<int, BenchmarkValue>? _cache;
    private int[]? _keys;
    private Random? _random;

    [Params(100, 1_000, 10_000)]
    public int DataSize { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _random = new Random(42); // Fixed seed for reproducibility
_keys = Enumerable.Range(0, DataSize).ToArray();
    }

    [IterationSetup]
    public void IterationSetup()
    {
        _cache = new InMemoryCache<int, BenchmarkValue>(3600);
 
    // Pre-populate cache with 50% of keys
    for (int i = 0; i < DataSize / 2; i++)
        {
    _cache.CreateEntry(_keys![i], new BenchmarkValue 
       { 
      Data = $"Value_{i}", 
         Timestamp = DateTime.UtcNow, 
      Counter = i 
      });
     }
    }

    [Benchmark(Baseline = true)]
    public void ReadHeavy_90Read_10Write()
    {
        // Typical web application pattern: mostly reads
      for (int i = 0; i < DataSize; i++)
  {
   if (i % 10 == 0)
       {
     // 10% writes
      var key = _keys![_random!.Next(DataSize)];
     _cache!.CreateEntry(key, new BenchmarkValue 
      { 
              Data = $"Updated_{key}", 
     Timestamp = DateTime.UtcNow, 
     Counter = key 
   });
         }
   else
  {
    // 90% reads
    var key = _keys![_random!.Next(DataSize)];
     _cache!.TryGetValue(key, out var _);
 }
        }
    }

    [Benchmark]
    public void WriteHeavy_70Write_30Read()
  {
   // Cache warming or bulk update scenario
    for (int i = 0; i < DataSize; i++)
  {
if (i % 10 < 7)
         {
    // 70% writes
    var key = _keys![_random!.Next(DataSize)];
    _cache!.CreateEntry(key, new BenchmarkValue 
       { 
      Data = $"Updated_{key}", 
      Timestamp = DateTime.UtcNow, 
      Counter = key 
             });
            }
  else
 {
       // 30% reads
       var key = _keys![_random!.Next(DataSize)];
          _cache!.TryGetValue(key, out var _);
   }
   }
    }

    [Benchmark]
    public void Balanced_CRUD_25Each()
    {
      // Balanced mix of all operations
    for (int i = 0; i < DataSize; i++)
  {
       var operation = i % 4;
      var key = _keys![_random!.Next(DataSize)];

     switch (operation)
    {
           case 0: // Create/Update
         _cache!.CreateEntry(key, new BenchmarkValue 
    { 
        Data = $"Value_{key}", 
           Timestamp = DateTime.UtcNow, 
    Counter = key 
       });
          break;
                case 1: // Read
    _cache!.TryGetValue(key, out var _);
            break;
  case 2: // Update (same as create)
     _cache!.CreateEntry(key, new BenchmarkValue 
   { 
     Data = $"Updated_{key}", 
         Timestamp = DateTime.UtcNow, 
        Counter = key * 2 
       });
       break;
              case 3: // Delete
    _cache!.Remove(key);
   break;
   }
   }
    }

    [Benchmark]
    public void SessionCache_Pattern()
    {
     // Simulates session cache: create, multiple reads, eventual remove
        var sessionsToSimulate = DataSize / 10;
      
     for (int session = 0; session < sessionsToSimulate; session++)
     {
            var sessionKey = _keys![_random!.Next(DataSize)];
      
     // Create session
   _cache!.CreateEntry(sessionKey, new BenchmarkValue 
   { 
    Data = $"Session_{sessionKey}", 
     Timestamp = DateTime.UtcNow, 
 Counter = sessionKey 
       });

     // Multiple reads (5-15 reads per session)
   var readsCount = _random.Next(5, 16);
      for (int read = 0; read < readsCount; read++)
     {
         _cache.TryGetValue(sessionKey, out var _);
  }

      // Optional update (30% chance)
       if (_random.Next(100) < 30)
            {
        _cache.CreateEntry(sessionKey, new BenchmarkValue 
       { 
       Data = $"Updated_Session_{sessionKey}", 
          Timestamp = DateTime.UtcNow, 
        Counter = sessionKey * 2 
         });
 }

   // Eventually remove (70% chance)
        if (_random.Next(100) < 70)
    {
            _cache.Remove(sessionKey);
          }
   }
    }

    [Benchmark]
    public void Cache_Churn_High()
    {
     // High churn rate: rapid create/remove cycles
     for (int i = 0; i < DataSize / 2; i++)
   {
  var key = _keys![i];
            
   // Create
  _cache!.CreateEntry(key, new BenchmarkValue 
   { 
            Data = $"Value_{key}", 
  Timestamp = DateTime.UtcNow, 
 Counter = key 
         });
        
  // Read a few times
   _cache.TryGetValue(key, out var _);
    _cache.TryGetValue(key, out var _);
            
       // Remove
          _cache.Remove(key);
        }
 }

    [Benchmark]
    public void Monitoring_Pattern()
    {
        // Simulates monitoring: periodic reads of all values + occasional updates
        for (int cycle = 0; cycle < 10; cycle++)
        {
   // Read all values (monitoring check)
        var allValues = _cache!.GetAllValues();
         
    // Update a few random entries
    for (int update = 0; update < DataSize / 20; update++)
     {
    var key = _keys![_random!.Next(DataSize)];
     _cache.CreateEntry(key, new BenchmarkValue 
    { 
Data = $"Monitored_{key}", 
           Timestamp = DateTime.UtcNow, 
  Counter = key 
   });
   }
        }
    }

    [Benchmark]
    public void LRU_Simulation()
  {
   // Simulates LRU-like access pattern with hot and cold data
     var hotDataSize = DataSize / 10; // 10% hot data
        var accessCount = DataSize * 2;

  for (int i = 0; i < accessCount; i++)
        {
// 80% of accesses go to hot data
       var isHotAccess = _random!.Next(100) < 80;
      var key = isHotAccess 
         ? _keys![_random.Next(hotDataSize)] 
       : _keys![_random.Next(DataSize)];

            if (_cache!.TryGetValue(key, out var value))
     {
         // Cache hit - optionally update
     if (_random.Next(100) < 5) // 5% update rate
       {
    _cache.CreateEntry(key, new BenchmarkValue 
      { 
       Data = $"Updated_{key}", 
      Timestamp = DateTime.UtcNow, 
      Counter = value?.Counter + 1 ?? 0 
         });
                }
            }
   else
 {
      // Cache miss - create entry
    _cache.CreateEntry(key, new BenchmarkValue 
     { 
          Data = $"Value_{key}", 
  Timestamp = DateTime.UtcNow, 
   Counter = 1 
        });
     }
  }
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _cache = null;
    }
}
