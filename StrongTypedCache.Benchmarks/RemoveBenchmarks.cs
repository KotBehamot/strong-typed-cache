using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using Cache;

namespace StrongTypedCache.Benchmarks;

/// <summary>
/// Benchmarks for Remove operation testing cache deletion performance.
/// Tests various removal patterns and their impact on cache performance.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(RunStrategy.Throughput, warmupCount: 3, iterationCount: 10)]
[MinColumn, MaxColumn, MeanColumn, MedianColumn]
[RankColumn]
public class RemoveBenchmarks
{
    private InMemoryCache<int, BenchmarkValue>? _intCache;
    private InMemoryCache<string, BenchmarkValue>? _stringCache;
    private int[]? _keys;
    private string[]? _stringKeys;
    private int[]? _randomRemovalPattern;

    [Params(10, 100, 1_000, 10_000)]
  public int DataSize { get; set; }

    [IterationSetup]
    public void IterationSetup()
    {
     // Recreate and populate cache for each iteration
        _intCache = new InMemoryCache<int, BenchmarkValue>(3600);
        _stringCache = new InMemoryCache<string, BenchmarkValue>(3600);
 
     _keys = Enumerable.Range(0, DataSize).ToArray();
        _stringKeys = Enumerable.Range(0, DataSize).Select(i => $"key_{i}").ToArray();

        // Populate caches
        for (int i = 0; i < DataSize; i++)
        {
       var value = new BenchmarkValue 
            { 
      Data = $"Value_{i}", 
                Timestamp = DateTime.UtcNow, 
           Counter = i 
            };
        _intCache.CreateEntry(_keys[i], value);
         _stringCache.CreateEntry(_stringKeys[i], value);
        }

        // Generate random removal pattern
    var random = new Random(42);
     _randomRemovalPattern = Enumerable.Range(0, DataSize)
            .OrderBy(_ => random.Next())
            .ToArray();
    }

    [Benchmark(Baseline = true)]
    public void Remove_IntKey_Sequential()
    {
   for (int i = 0; i < DataSize; i++)
     {
        _intCache!.Remove(_keys![i]);
        }
  }

    [Benchmark]
    public void Remove_IntKey_Random()
    {
      for (int i = 0; i < DataSize; i++)
        {
  _intCache!.Remove(_keys![_randomRemovalPattern![i]]);
        }
    }

    [Benchmark]
    public void Remove_StringKey_Sequential()
    {
        for (int i = 0; i < DataSize; i++)
        {
      _stringCache!.Remove(_stringKeys![i]);
}
    }

    [Benchmark]
    public void Remove_IntKey_Alternate()
    {
        // Remove every other entry
        for (int i = 0; i < DataSize; i += 2)
        {
  _intCache!.Remove(_keys![i]);
        }
    }

    [Benchmark]
    public void Remove_IntKey_FirstHalf()
    {
        // Remove first half of entries
      for (int i = 0; i < DataSize / 2; i++)
        {
    _intCache!.Remove(_keys![i]);
        }
    }

    [Benchmark]
    public void Remove_IntKey_LastHalf()
    {
        // Remove last half of entries
        for (int i = DataSize / 2; i < DataSize; i++)
        {
    _intCache!.Remove(_keys![i]);
        }
    }

    [Benchmark]
    public void Remove_NonExistent_Keys()
    {
      // Test removing keys that don't exist
        for (int i = DataSize; i < DataSize * 2; i++)
        {
         _intCache!.Remove(i);
        }
    }

    [Benchmark]
    public void Remove_And_Reinsert()
    {
      // Realistic pattern: remove and immediately reinsert
  for (int i = 0; i < DataSize / 2; i++)
  {
            _intCache!.Remove(_keys![i]);
            _intCache.CreateEntry(_keys[i], new BenchmarkValue 
  { 
   Data = $"New_Value_{i}", 
       Timestamp = DateTime.UtcNow, 
                Counter = i * 2 
       });
        }
    }

    [GlobalCleanup]
    public void Cleanup()
    {
   _intCache = null;
        _stringCache = null;
    }
}
