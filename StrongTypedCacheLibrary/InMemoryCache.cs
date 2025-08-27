using System.Collections;
using System.Reflection;
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
    private readonly TimeSpan AbsoluteExpirationTimeSec;
    private readonly IMemoryCache MemoryCacheInstance;
    private readonly PropertyInfo EntriesCollectionProperty;
    private readonly PropertyInfo ValueProperty;

    public TimeSpan AbsoluteExpiration => AbsoluteExpirationTimeSec;
    public IMemoryCache MemoryCache => MemoryCacheInstance;
    public PropertyInfo EntriesCollection => EntriesCollectionProperty;
    public PropertyInfo ValueProp => ValueProperty;

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryCache{TKey, TValue}"/> class.
    /// </summary>
    /// <param name="absoluteExpirationTimeSec">Absolute expiration time in seconds (default: 3600).</param>
    public InMemoryCache(int absoluteExpirationTimeSec = 3600)
    {
        var options = new MemoryCacheOptions { Clock = new SystemClock() };
        MemoryCacheInstance = new MemoryCache(options);
        AbsoluteExpirationTimeSec = TimeSpan.FromSeconds(absoluteExpirationTimeSec);
        EntriesCollectionProperty = typeof(MemoryCache).GetProperty("EntriesCollection", BindingFlags.NonPublic | BindingFlags.Instance);
        ValueProperty = typeof(TValue).GetProperty("value");
    }

    /// <inheritdoc />
    public List<TValue?> GetAllValues()
    {
        var collection = EntriesCollectionProperty.GetValue(MemoryCacheInstance) as ICollection;
        var items = new List<TValue?>();
        if (collection != null)
        {
            foreach (var item in collection)
            {
                var val = ValueProperty.GetValue(item);
                items.Add((TValue?)val);
            }
        }
        return items;
    }

    /// <inheritdoc />
    public bool TryGetValue(TKey key, out TValue value)
    {
        return MemoryCacheInstance.TryGetValue(key, out value);
    }

    /// <inheritdoc />
    public bool CreateEntry(TKey key, TValue value)
    {
        if (key is null) throw new ArgumentNullException(nameof(key));
        if (value is null) throw new ArgumentNullException(nameof(value));
        var entry = MemoryCacheInstance.CreateEntry(key);
        entry.SetValue(value);
        entry.SetAbsoluteExpiration(AbsoluteExpirationTimeSec);
        return true;
    }

    /// <inheritdoc />
    public void Remove(TKey key)
    {
        if (key is null) throw new ArgumentNullException(nameof(key));
        MemoryCacheInstance.Remove(key);
    }
}