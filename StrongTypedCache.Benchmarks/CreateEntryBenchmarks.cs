using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using Cache;

namespace StrongTypedCache.Benchmarks;

/// <summary>
/// Benchmarks for CreateEntry operation across different key types and data volumes.
/// Tests cache write performance with various scenarios.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(RunStrategy.Throughput, warmupCount: 3, iterationCount: 10)]
[MinColumn, MaxColumn, MeanColumn, MedianColumn]
[RankColumn]
public class CreateEntryBenchmarks
{
    private InMemoryCache<int, BenchmarkValue>? _intCache;
    private InMemoryCache<string, BenchmarkValue>? _stringCache;
    private InMemoryCache<Guid, BenchmarkValue>? _guidCache;
    private InMemoryCache<ComplexKey, BenchmarkValue>? _complexCache;
    
    private int[]? _intKeys;
    private string[]? _stringKeys;
    private Guid[]? _guidKeys;
    private ComplexKey[]? _complexKeys;
    private BenchmarkValue[]? _values;

    [Params(10, 100, 1_000, 10_000)]
    public int DataSize { get; set; }

    [GlobalSetup]
    public void Setup()
    {
  // Initialize caches with long expiration
        _intCache = new InMemoryCache<int, BenchmarkValue>(3600);
        _stringCache = new InMemoryCache<string, BenchmarkValue>(3600);
        _guidCache = new InMemoryCache<Guid, BenchmarkValue>(3600);
        _complexCache = new InMemoryCache<ComplexKey, BenchmarkValue>(3600);

        // Pre-generate keys and values
        _intKeys = Enumerable.Range(0, DataSize).ToArray();
   _stringKeys = Enumerable.Range(0, DataSize).Select(i => $"key_{i}").ToArray();
        _guidKeys = Enumerable.Range(0, DataSize).Select(_ => Guid.NewGuid()).ToArray();
    _complexKeys = Enumerable.Range(0, DataSize)
            .Select(i => new ComplexKey { Id = i, Name = $"Name_{i}", Timestamp = DateTime.UtcNow })
            .ToArray();
   _values = Enumerable.Range(0, DataSize)
     .Select(i => new BenchmarkValue { Data = $"Value_{i}", Timestamp = DateTime.UtcNow, Counter = i })
      .ToArray();
 }

    [Benchmark(Baseline = true)]
    public void CreateEntry_IntKey()
    {
        for (int i = 0; i < DataSize; i++)
        {
            _intCache!.CreateEntry(_intKeys![i], _values![i]);
    }
    }

    [Benchmark]
    public void CreateEntry_StringKey()
    {
  for (int i = 0; i < DataSize; i++)
        {
            _stringCache!.CreateEntry(_stringKeys![i], _values![i]);
        }
    }

    [Benchmark]
    public void CreateEntry_GuidKey()
    {
  for (int i = 0; i < DataSize; i++)
  {
          _guidCache!.CreateEntry(_guidKeys![i], _values![i]);
        }
    }

    [Benchmark]
    public void CreateEntry_ComplexKey()
    {
      for (int i = 0; i < DataSize; i++)
        {
            _complexCache!.CreateEntry(_complexKeys![i], _values![i]);
 }
    }

    [Benchmark]
    public void CreateEntry_Overwrite_IntKey()
    {
 // Test overwrite scenario - write same keys multiple times
    for (int pass = 0; pass < 3; pass++)
  {
        for (int i = 0; i < DataSize; i++)
 {
       _intCache!.CreateEntry(_intKeys![i], _values![i]);
            }
        }
    }

    [GlobalCleanup]
public void Cleanup()
    {
      _intCache = null;
  _stringCache = null;
        _guidCache = null;
        _complexCache = null;
  }
}

public class BenchmarkValue
{
    public string Data { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
public int Counter { get; set; }
}

public class ComplexKey : IEquatable<ComplexKey>
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }

    public bool Equals(ComplexKey? other)
  {
        if (other is null) return false;
        return Id == other.Id && Name == other.Name;
    }

    public override bool Equals(object? obj) => Equals(obj as ComplexKey);

    public override int GetHashCode() => HashCode.Combine(Id, Name);
}
