using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Internal;
using StrongTypedCache.Abstractions;

namespace Cache;

/// <summary>
/// In-memory implementation of the <see cref="ICache{TKey, TValue}"/> interface.
/// </summary>
/// <typeparam name="TKey">Type of the cache key.</typeparam>
/// <typeparam name="TValue">Type of the cache value.</typeparam>
public class InMemoryCache<TKey, TValue> : ICache<TKey, TValue>
    where TValue : new()
{
    private readonly TimeSpan _absoluteExpiration;
    private readonly IMemoryCache _memoryCache;
    private readonly ConcurrentDictionary<TKey, byte> _keys = new();

    public TimeSpan AbsoluteExpiration => _absoluteExpiration;
    public IMemoryCache MemoryCache => _memoryCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryCache{TKey, TValue}"/> class.
    /// </summary>
    /// <param name="absoluteExpirationTimeSec">Absolute expiration time in seconds (default: 3600).</param>
    public InMemoryCache(int absoluteExpirationTimeSec = 3600)
    {
        var options = new MemoryCacheOptions { Clock = new SystemClock() };
        _memoryCache = new MemoryCache(options);
        _absoluteExpiration = TimeSpan.FromSeconds(absoluteExpirationTimeSec);
    }

    /// <inheritdoc />
    public List<TValue?> GetAllValues()
    {
        var items = new List<TValue?>();
        foreach (var key in _keys.Keys)
        {
            if (_memoryCache.TryGetValue(key!, out TValue value))
            {
                items.Add(value);
            }
            else
            {
                // Clean up expired/missing keys
                _keys.TryRemove(key, out _);
            }
        }
        return items;
    }

    /// <inheritdoc />
    public bool TryGetValue(TKey key, out TValue value)
    {
        return _memoryCache.TryGetValue(key!, out value!);
    }

    /// <inheritdoc />
    public bool CreateEntry(TKey key, TValue value)
    {
        if (key is null) throw new ArgumentNullException(nameof(key));
        if (value is null) throw new ArgumentNullException(nameof(value));

        using (var entry = _memoryCache.CreateEntry(key))
        {
            entry.SetValue(value);
            entry.SetAbsoluteExpiration(_absoluteExpiration);
        } // disposing commits the entry to the cache

        _keys[key] = 0; // track key
        return true;
    }

    /// <inheritdoc />
    public void Remove(TKey key)
    {
        if (key is null) throw new ArgumentNullException(nameof(key));
        _memoryCache.Remove(key);
        _keys.TryRemove(key, out _);
    }
}