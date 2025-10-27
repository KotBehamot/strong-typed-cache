using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using Cache;

namespace StrongTypedCache.Benchmarks;

/// <summary>
/// Benchmarks for TryGetValue operation testing cache read performance.
/// Covers cache hits, misses, and sequential vs random access patterns.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(RunStrategy.Throughput, warmupCount: 3, iterationCount: 10)]
[MinColumn, MaxColumn, MeanColumn, MedianColumn]
[RankColumn]
public class TryGetValueBenchmarks
{
    private InMemoryCache<int, BenchmarkValue>? _intCache;
    private InMemoryCache<string, BenchmarkValue>? _stringCache;
    private InMemoryCache<Guid, BenchmarkValue>? _guidCache;
    
    private int[]? _existingIntKeys;
    private int[]? _missingIntKeys;
  private string[]? _existingStringKeys;
    private string[]? _missingStringKeys;
    private Guid[]? _existingGuidKeys;
    private Guid[]? _missingGuidKeys;
    private int[]? _randomAccessPattern;

    [Params(10, 100, 1_000, 10_000)]
    public int DataSize { get; set; }

    [GlobalSetup]
    public void Setup()
    {
  // Initialize caches
        _intCache = new InMemoryCache<int, BenchmarkValue>(3600);
        _stringCache = new InMemoryCache<string, BenchmarkValue>(3600);
        _guidCache = new InMemoryCache<Guid, BenchmarkValue>(3600);

        // Populate caches with test data
        _existingIntKeys = Enumerable.Range(0, DataSize).ToArray();
      _existingStringKeys = Enumerable.Range(0, DataSize).Select(i => $"key_{i}").ToArray();
        _existingGuidKeys = Enumerable.Range(0, DataSize).Select(_ => Guid.NewGuid()).ToArray();

        for (int i = 0; i < DataSize; i++)
        {
     var value = new BenchmarkValue 
            { 
       Data = $"Value_{i}", 
       Timestamp = DateTime.UtcNow, 
         Counter = i 
            };
     _intCache.CreateEntry(_existingIntKeys[i], value);
       _stringCache.CreateEntry(_existingStringKeys[i], value);
            _guidCache.CreateEntry(_existingGuidKeys[i], value);
        }

        // Generate keys that don't exist in cache
        _missingIntKeys = Enumerable.Range(DataSize, DataSize).ToArray();
        _missingStringKeys = Enumerable.Range(DataSize, DataSize).Select(i => $"missing_{i}").ToArray();
        _missingGuidKeys = Enumerable.Range(0, DataSize).Select(_ => Guid.NewGuid()).ToArray();

        // Generate random access pattern
   var random = new Random(42); // Fixed seed for reproducibility
        _randomAccessPattern = Enumerable.Range(0, DataSize)
            .OrderBy(_ => random.Next())
     .ToArray();
    }

    [Benchmark(Baseline = true)]
    public void TryGetValue_IntKey_AllHits_Sequential()
    {
   for (int i = 0; i < DataSize; i++)
    {
    _intCache!.TryGetValue(_existingIntKeys![i], out var value);
   }
    }

    [Benchmark]
    public void TryGetValue_IntKey_AllHits_Random()
    {
        for (int i = 0; i < DataSize; i++)
      {
            _intCache!.TryGetValue(_existingIntKeys![_randomAccessPattern![i]], out var value);
    }
    }

    [Benchmark]
    public void TryGetValue_IntKey_AllMisses()
    {
        for (int i = 0; i < DataSize; i++)
        {
    _intCache!.TryGetValue(_missingIntKeys![i], out var value);
        }
    }

    [Benchmark]
    public void TryGetValue_IntKey_Mixed_50Percent_Hits()
    {
      for (int i = 0; i < DataSize; i++)
 {
         var key = i % 2 == 0 ? _existingIntKeys![i / 2] : _missingIntKeys![i / 2];
    _intCache!.TryGetValue(key, out var value);
        }
    }

    [Benchmark]
    public void TryGetValue_StringKey_AllHits()
    {
        for (int i = 0; i < DataSize; i++)
     {
        _stringCache!.TryGetValue(_existingStringKeys![i], out var value);
        }
}

    [Benchmark]
    public void TryGetValue_StringKey_AllMisses()
 {
        for (int i = 0; i < DataSize; i++)
{
         _stringCache!.TryGetValue(_missingStringKeys![i], out var value);
        }
    }

    [Benchmark]
    public void TryGetValue_GuidKey_AllHits()
    {
        for (int i = 0; i < DataSize; i++)
        {
_guidCache!.TryGetValue(_existingGuidKeys![i], out var value);
    }
    }

    [Benchmark]
    public void TryGetValue_GuidKey_AllMisses()
    {
        for (int i = 0; i < DataSize; i++)
        {
      _guidCache!.TryGetValue(_missingGuidKeys![i], out var value);
        }
    }

    [Benchmark]
    public void TryGetValue_IntKey_Repeated_SameKey()
    {
        // Test cache locality - repeatedly accessing same key
        var key = _existingIntKeys![0];
        for (int i = 0; i < DataSize; i++)
  {
   _intCache!.TryGetValue(key, out var value);
        }
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _intCache = null;
      _stringCache = null;
        _guidCache = null;
    }
}
